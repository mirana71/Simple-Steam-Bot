using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using SteamKit2;

namespace simplesteambot
{
	// Can later change to Action<callback, options, arguments) if it makes a difference
	public delegate void Command(SteamKit2.SteamFriends.FriendMsgCallback callback, string options, string[] arguments);

	public static class CommandList
	{
		public static Dictionary<string, Command> RegularList = RegularCommands();
		public static Dictionary<string, Command> AdminList = AdminCommands();

		/* Command template
		 * 
		commands.Add("!.", (callback, options, arguments) =>
		{

		});
		*/

		private static Dictionary<string, Command> RegularCommands()
		{
			Dictionary<string, Command> commands = new Dictionary<string, Command>();

			#region Commands

			commands.Add("!help", (callback, options, arguments) =>
			{
                Bot.SendChat(callback.Sender, "Доступные команды:\n" + // Optional parts of a command potentially? Best way to indicate command use?
                    "!reminder (временно недоступно)\n !protein\n !goshaloh\n !vk\n !dendiboss\n");
                    
			});

			commands.Add("!protein", (callback, options, arguments) =>
			{
				Bot.SendChat(callback.Sender, "Протеин атлант!");
                Bot.SendChat(callback.Sender, "Протеин атлант!");
                Bot.SendChat(callback.Sender, "Протеин атлант!");
                Bot.SendChat(callback.Sender, "Протеин атлант!");
                Bot.SendChat(callback.Sender, "Протеин атлант!");
                Bot.SendChat(callback.Sender, "На массу!!!");
                Bot.SendChat(callback.Sender, "На массу!!!");
                Task.Delay(TimeSpan.FromSeconds(2)).ContinueWith(x =>
                {
                    Bot.SendChat(callback.Sender, "Не учел, что нужно еще и на сушку..");
                    Bot.SendChat(callback.Sender, "В любом случае лучший протеин можно купить здесь: atlant-sport.ru/proteini/product-8196");
                });

            });

			commands.Add("!goshaloh", (callback, options, arguments) =>
			{
				Bot.SendChat(callback.Sender, "Вероятность того, что Гоша loh равна...");
                Bot.SendChat(callback.Sender, "1");
			});

			commands.Add("!reminder", (callback, options, arguments) =>
			{
				if (arguments.Length < 2)
				{
					Bot.SendChat(callback.Sender, "Нет достаточных аргументов для !reminder.\n" +
						"Убедитесь, что вы ввели аргументы правильно: !reminder <time> <-minutes or -hours> <message>");
					return;
				}

				float time = 0;
				string unit = "";
				double totalMinutes = 0;

				try
				{
					time = float.Parse(arguments[0], System.Globalization.CultureInfo.InvariantCulture);
				}
				catch
				{
					Bot.SendChat(callback.Sender, "Неправильный формат набора.\n" +
						"Первое значение должно быть указано после команды !reminder.");
					return;
				}

				if (time < 1 || time > 120)
				{
					Bot.SendChat(callback.Sender, "Установленное время не может быть меньше 1 или больше 120!");
					return;
				}

				if (options.Contains('m'))
				{
					unit = "minutes";
					totalMinutes = time;
				}
				else if (options.Contains('h'))
				{
					unit = "hours";
					totalMinutes = time * 60;
				}
				else
				{
					Bot.SendChat(callback.Sender, "Установили время\n" +
						"Убедитесь, что вы вводите -hours -minutes");
					return;
				}

				if (time == 1)
				{
					unit = unit.Substring(0, unit.Length - 1);
				}


				var str = new StringBuilder();

				for (int i = 1; i < arguments.Length; i++)
				{
					str.Append($"{arguments[i]} ");
				}

				if (string.IsNullOrWhiteSpace(str.ToString()))
				{
					str.Append("Нет сообщения!");
				}


				var user = new User
				{
					userID = callback.Sender,
					timer = new Timer(TimeSpan.FromMinutes(totalMinutes).TotalMilliseconds),
					reminder = new Reminder
					{
						message = str.ToString(),
					}
				};

				user.timer.Elapsed += (sender, e) => Bot.HandleReminder(user);
				user.timer.AutoReset = false;
				user.timer.Enabled = true;


				Bot.SendChat(callback.Sender, $"Напоминалка установлена. Увидимся в {time} {unit}!");
			});

            commands.Add("!vk", (callback, options, arguments) =>
            {
                Bot.SendChat(callback.Sender, "Ссылка на вк: [censored]");
            });

            commands.Add("!dendiboss", (callback, options, arguments) =>
            {
                Bot.SendChat(callback.Sender, "Одну минуту..");
                Task.Delay(TimeSpan.FromSeconds(2)).ContinueWith(oo =>
                {
                    Bot.SendChat(callback.Sender, "Это он: steamcommunity.com/id/DendiQ/");
                    Bot.SendChat(callback.Sender, "Вот его SteamID64: 76561198030654385");
                });
            });

            //commands.Add("!rich", (callback, options, arguments) =>



            #endregion

            return commands;
		}

		/*public static Dictionary<string, Command> ElevatedCommands()
		{
			Dictionary<string, Command> commands = new Dictionary<string, Command>();

			return commands;
		}*/

		private static Dictionary<string, Command> AdminCommands()
		{
			Dictionary<string, Command> commands = new Dictionary<string, Command>();

			#region Commands

			commands.Add(".", (callback, options, arguments) =>
			{
				Bot.SendChat(callback.Sender, $"Привет админ! Welcome back {callback.Sender.AccountID}.");
			});

			commands.Add(".help", (callback, options, arguments) =>
			{
				Bot.SendChat(callback.Sender, "Команды админа:\n" +
					".shutdown -\\-\\ Выключить бота.\n" + 
					".log -\\-\\ Писать лог бота в консоль\n" +
					".echo -\\-\\ Эхо для сообщений\n");
			});

			commands.Add(".shutdown", (callback, options, arguments) =>
			{
				Bot.SendChat(callback.Sender, "Всем пока я спать...");
				Bot.steamUser.LogOff();
			});

			commands.Add(".restart", (callback, options, arguments) =>
			{
				Bot.SendChat(callback.Sender, "Запускаю рестарт бота...");
			});
			
			commands.Add(".log", (callback, options, arguments) =>
			{
				Console.WriteLine($"{callback.Sender} at {System.DateTime.Now}: {Utility.CompressStrings(arguments)}");
				Bot.SendChat(callback.Sender, "Сообщение записано в лог.");
			});
			
			commands.Add(".echo", (callback, options, arguments) =>
			{
				Bot.SendChat(callback.Sender, Utility.CompressStrings(arguments));
			});

			//commands.Add(".debug", (callback, options, arguments) =>
			//{
				//var args = new StringBuilder();
				//foreach (string arg in arguments)
				//{
					//args.Append($"{arg}\n");
				//}
				//Bot.SendChat(callback.Sender, $"Options:\n{options}\n\nArguments:\n{args}");
			//});

			#endregion

			return commands;
		}
	}
}
