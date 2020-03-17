﻿using MiniIT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using UnityEngine.Networking;

namespace MiniIT.Snipe
{
	public class SnipeTable
	{
		public static string Path;

		internal IEnumerator LoadTableCoroutine(string table_name)
		{
			string url = string.Format("{0}/{1}.json.gz", Path, table_name);
			UnityEngine.Debug.Log("[SnipeTable] Loading table " + url);

			using (UnityWebRequest loader = new UnityWebRequest(url))
			{
				loader.downloadHandler = new DownloadHandlerBuffer();
				yield return loader.SendWebRequest();
				if (loader.isNetworkError || loader.isHttpError)
				{
					UnityEngine.Debug.Log("[SnipeTable] Network error: Failed to load table - " + table_name);
				}
				else
				{
					UnityEngine.Debug.Log("[SnipeTable] table file loaded - " + table_name);
					try
					{
						if (loader.downloadHandler.data == null || loader.downloadHandler.data.Length < 1)
						{
							UnityEngine.Debug.Log("[SnipeTable] Error: loaded data is null or empty. Table: " + table_name);
						}
						else
						{
							using (GZipStream gzip = new GZipStream(new MemoryStream(loader.downloadHandler.data, false), CompressionMode.Decompress))
							{
								using (StreamReader reader = new StreamReader(gzip))
								{
									string json_string = reader.ReadToEnd();
									ExpandoObject data = ExpandoObject.FromJSONString(json_string);

									if (data["list"] is List<object> list)
									{
										foreach (ExpandoObject item_data in list)
										{
											AddTableItem(item_data);
										}
									}

									UnityEngine.Debug.Log("[SnipeTable] table ready - " + table_name);
								}
							}
						}
					}
					catch (Exception)
					{
						UnityEngine.Debug.Log("[SnipeTable] failed to parse table - " + table_name);
					}
				}
			}
		}

		protected virtual void AddTableItem(ExpandoObject item_data)
		{
			// override this method
		}
	}
}