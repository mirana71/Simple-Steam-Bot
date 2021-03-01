using System;
using System.Collections.Generic;
using System.Text;

namespace simplesteambot
{
	static class Utility
	{
		public static string GetHiddenConsoleInput()
		{
			StringBuilder input = new StringBuilder();
			while (true)
			{
				var key = Console.ReadKey(true);
				if (key.Key == ConsoleKey.Enter)
				{
					break;
				}
				else if (key.Key == ConsoleKey.Backspace && input.Length > 0)
				{
					input.Remove(input.Length - 1, 1);
				}
				else if (key.Key != ConsoleKey.Backspace)
				{
					input.Append(key.KeyChar);
				}
			}
			return input.ToString();
		}

		public static void SplitCommand(out string command, out string options, out string[] arguments, string message)
		{
			int index = 0;
			options = "";

			foreach (char c in message)
			{
				if (char.IsWhiteSpace(c))
				{
					break;
				}

				index += 1;
			}

			command = message.Substring(0, index);

			List<string> args = new List<string>();

			var str = new StringBuilder();

			bool invertedCommas = false, option = false;

			index += 1;

			for (; index < message.Length; index++)
			{
				if (message[index] == '-' && !invertedCommas && message[index - 1] == ' ') // [index - 1] might not be safe! Though theoretically should be fine
				{ // To note, with this implementation, cannot do words as options or risk letters falsely identifying other options.
					option = true;

					continue;
				}

				if (message[index] == '\"' && !option)
				{
					invertedCommas = !invertedCommas;

					if (!invertedCommas) // End of phrase
					{
						args.Add(str.ToString());

						str.Clear();

						index += 1;
					}

					continue;
				}
				else if (char.IsWhiteSpace(message[index]) && !invertedCommas)
				{
					if (option)
					{
						option = false;

						continue;
					}

					args.Add(str.ToString());

					str.Clear();

					continue;
				}

				if (option)
				{
					options += message[index];
				}
				else
				{
					str.Append(message[index]);
				}
			}

			args.Add(str.ToString());

			arguments = args.ToArray();
		}

		public static string CompressStrings(params string[] strings)
		{
			var str = new StringBuilder();

			foreach (string arg in strings)
			{
				str.Append($"{arg} ");
			}

			return str.ToString();
		}

		public static string Sanitise()
		{
			// TODO, do I need to though?
			
			return "";
		}
	}
}
