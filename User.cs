using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace simplesteambot
{
	public class User : IDisposable
	{
		public SteamKit2.SteamID userID;

		public Timer timer;

		public Reminder? reminder;

		#region Disposing

		private bool disposed = false;

		public void Dispose()
		{
			Dispose(true);

			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					timer.Dispose();
				}

				userID = null;
				reminder = null;

				disposed = true;
			}
		}

		~User()
		{
			Dispose(false);
		}

		#endregion
	}


	public struct Reminder
	{
		public string message;
	}
}
