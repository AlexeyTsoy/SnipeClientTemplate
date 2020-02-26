﻿using System;
using UnityEngine;

namespace MiniIT.Snipe
{
	public class AuthProvider : IDisposable
	{
		public const string ERROR_OK = "ok";
		public const string ERROR_NOT_INITIALIZED = "notInitialized";
		public const string ERROR_NO_SUCH_USER = "noSuchUser";
		public const string ERROR_NO_SUCH_AUTH = "noSuchAuth";
		public const string ERROR_PARAMS_WRONG = "paramsWrong";

		protected const string REQUEST_USER_LOGIN = "auth/user.login";
		protected const string REQUEST_USER_BIND = "auth/user.bind";
		protected const string REQUEST_USER_EXISTS = "auth/user.exists";

		public virtual string ProviderId { get { return "__"; } }
		
		protected Action<int, string> mAuthSucceesCallback;
		protected Action<string> mAuthFailCallback;

		public virtual void Dispose()
		{
			mAuthSucceesCallback = null;
			mAuthFailCallback = null;
		}

		public virtual void RequestAuth(Action<int, string> success_callback, Action<string> fail_callback, bool reset_auth = false)
		{
			// Override this method.

			//mAuthSucceesCallback = success_callback;
			//mAuthFailCallback = fail_callback;

			InvokeAuthFailCallback(ERROR_NOT_INITIALIZED);
		}

		protected void RequestLogin(string provider, string login, string token, bool reset_auth = false)
		{
			ExpandoObject data = new ExpandoObject()
			{
				["messageType"] = REQUEST_USER_LOGIN,
				["provider"] = provider,
				["login"] = login,
				["auth"] = token,
			};
			if (reset_auth)
				data["resetInternalAuth"] = reset_auth;

			SingleRequestClient.Request(SnipeConfig.Instance.auth, data, (response) =>
			{
				string error_code = response?.SafeGetString("errorCode");
				if (error_code == "ok")
				{
					OnAuthLoginResponse(response);
				}
				else
				{
					InvokeAuthFailCallback(error_code);
				}
			});
		}

		protected virtual void OnAuthLoginResponse(ExpandoObject data)
		{
			string auth_login = data?.SafeGetString("internalUID");
			string auth_token = data?.SafeGetString("internalPassword");

			if (!string.IsNullOrEmpty(auth_login) && !string.IsNullOrEmpty(auth_token))
			{
				PlayerPrefs.SetString(SnipePrefs.AUTH_UID, auth_login);
				PlayerPrefs.SetString(SnipePrefs.AUTH_KEY, auth_token);
			}
		}

		protected virtual void InvokeAuthSuccessCallback(int user_id, string login_token)
		{
			if (mAuthSucceesCallback != null)
				mAuthSucceesCallback.Invoke(user_id, login_token);

			mAuthSucceesCallback = null;
			mAuthFailCallback = null;
		}

		protected virtual void InvokeAuthFailCallback(string error_code)
		{
			if (mAuthFailCallback != null)
				mAuthFailCallback.Invoke(error_code);

			mAuthSucceesCallback = null;
			mAuthFailCallback = null;
		}
	}
}