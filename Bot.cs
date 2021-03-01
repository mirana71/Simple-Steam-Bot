using System;
using System.Collections.Generic;
using System.Text;
using SteamKit2;

namespace simplesteambot
{
	public class Bot
	{
		#region Variables

		#region Consts

		const string UnrecognisedMessage = "Такой команды нет, убедись, что ! и команда написана правильно.\nЕсли же не знаешь, то просто напиши !help!";

		#endregion

		#region Global

		public static Bot instance = null;

		public static SteamClient steamClient = new SteamClient();
		public static CallbackManager manager;
		public static SteamUser steamUser;
		public static SteamFriends steamFriends;

        



		//public static List<User> activeUsers = new List<User>(); // Maybe change to dict later. Would be good to overwrite if multiple of same user

		#endregion

		private string username, password;
		private bool isRunning;

		private List<SteamID> admins;




		#endregion


		#region Setup and Teardown

		public void StartUp()
		{
			if (instance == null)
			{
				instance = this;
			}
			else
			{
				Console.WriteLine($"ERROR: БОТ УЖЕ ЗАПУЩЕН.");
				return;
			}

			// Info

			LoginInfo? loginInfo = FileManager.GetSecret();

			if (loginInfo.HasValue)
			{
				Console.WriteLine("Пытаюсь зайти с secret.txt");

				username = loginInfo.Value.user;
				password = loginInfo.Value.pass;
			}
			else
			{
				Console.WriteLine("Невозможно загрузить secret.txt.");

				Console.Write("Username: ");

				username = Console.ReadLine();

				Console.Write("Password: ");

				password = Console.ReadLine(); ;

				Console.WriteLine();
			}


			// File

			admins = FileManager.GetAdmins();


			// Connection

			manager = new CallbackManager(steamClient);

			steamUser = steamClient.GetHandler<SteamUser>();

			steamFriends = steamClient.GetHandler<SteamFriends>();

			#region Callbacks

			manager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
			manager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);

			manager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
			manager.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);
			manager.Subscribe<SteamUser.AccountInfoCallback>(OnAccountInfo);

			manager.Subscribe<SteamFriends.FriendsListCallback>(OnFriendsList);
			manager.Subscribe<SteamFriends.FriendMsgCallback>(OnFriendMsg);

			#endregion

			Console.WriteLine("Захожу в стим...");

			steamClient.Connect();

			isRunning = true;

			while(isRunning)
			{
				manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
			}

			Console.WriteLine("Бот выключен!");
		}

		void OnConnected(SteamClient.ConnectedCallback callback)
		{
			Console.WriteLine($"Зашел в стим.\nЛогинюсь в {username}...");

			steamUser.LogOn(new SteamUser.LogOnDetails
			{
				Username = username,
				Password = password

			});
		}

        
		void OnDisconnected(SteamClient.DisconnectedCallback callback)
		{
			Console.WriteLine($"Дисконектнулся..");

			isRunning = false;
		}

		void OnLoggedOn(SteamUser.LoggedOnCallback callback)
		{
			if(callback.Result != EResult.OK)
			{
				switch(callback.Result)
				{
					case EResult.AccountLogonDenied:
						Console.WriteLine("ОШИБКА: Не зашел в аккаунт, нужен стимгуард.");
						break;
					default:
						Console.WriteLine($"ОШИБКА: Не могу зайти в аккаунт!\n{callback.Result}\n{callback.ExtendedResult}");
						break;
				}

				isRunning = false;
				return;
			}

			Console.WriteLine("Успешно зашел в аккаунт!");
		}

		void OnLoggedOff(SteamUser.LoggedOffCallback callback)
		{
			Console.WriteLine($"Успешно вышел из аккаунта! {callback.Result}");
		}

		#endregion

		void OnAccountInfo(SteamUser.AccountInfoCallback callback) // Получаем аккаунт инфо
		{
			Console.WriteLine($"AccountInfo получен. {callback.PersonaName} теперь активный.");

			steamFriends.SetPersonaState(EPersonaState.Online); // ** offline mode works as well !
            steamFriends.SetPersonaName("[unknown]");
		}

		void OnFriendsList(SteamFriends.FriendsListCallback callback) // Получаем список друзей в аккаунте бота
		{
			foreach (var friend in callback.FriendList)
			{
				Console.WriteLine($"Друзья: {friend.SteamID}");

				if (friend.Relationship == EFriendRelationship.RequestRecipient)
				{
					steamFriends.AddFriend(friend.SteamID);
				}
			}
		}

		void OnFriendMsg(SteamFriends.FriendMsgCallback callback) // 
		{
			SteamID sender = callback.Sender;

			if (callback.EntryType == EChatEntryType.ChatMsg)
			{

				Utility.SplitCommand(out string commandName, out string options, out string[] arguments, callback.Message);

				commandName = commandName.ToLower();

				#region Message Handling


				Command command;

				if (commandName.StartsWith("!") && CommandList.RegularList.TryGetValue(commandName, out command))
				{
					command(callback, options, arguments);
				}
				else if (commandName.StartsWith("."))
				{
					if(admins.Find(id => id.AccountID == sender.AccountID) != null)
					{
						if (CommandList.AdminList.TryGetValue(commandName, out command))
						{
							command(callback, options, arguments);
						}
						else
						{
							SendChat(sender, "Команда не найдена.");
						}
					}
					else
					{
						SendChat(sender, $"У вас нет прав для использования этой команды.");
					}
				}
				else
				{
					SendChat(sender, UnrecognisedMessage);
				}
				

				#endregion
			}

		}


		#region Handling

		public static void HandleReminder(User user)
		{
			if (user != null)
			{
				SendChat(user.userID, $"Напоминалка: {user.reminder.Value.message}");

				user.timer.Enabled = false;
				user.Dispose();
			}
			else
			{
				Console.WriteLine("Не получилось привязать напоминалку.");
			}

		}


		#endregion


		#region Actions

		public static void SendChat(SteamID target, string message)
		{
			steamFriends.SendChatMessage(target, EChatEntryType.ChatMsg, message);
		}

		// Mass announcement to friends list action? Risky.

		#endregion

	}
}
