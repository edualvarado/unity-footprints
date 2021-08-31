using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace UPyPlot {
	public class UPyPlotController : MonoBehaviour {

		[SerializeField] private string plotFileName = "plot.txt";
		[Tooltip("Name of the plot data file (Must Match Setting In Python File!!).")]
		private string absoluteName;

		[Range(0,4)]
		[Tooltip("Number of decimal places for each value in the plot data file.")]
		[SerializeField] private int precision = 2; // How many decimal places to add when storing values in the plot data file.

		[Range(0.01f,1.0f)]
		[Tooltip("Rate in seconds which to update plot data file with new values.")]
		[SerializeField] private float interval = 0.1f; // How often to update the data in the plot data file.

		[Range(2,1000)]
		[Tooltip("Maximum lines/history that the plot file should contain.")]
		[SerializeField] private int maxSamples = 25;
		private int currentSample = 0;

		private List<FieldInfo> probes = new List<FieldInfo>();
		private List<MonoBehaviour> monos = new List<MonoBehaviour>();

		[AttributeUsage(AttributeTargets.Field)]
		public class UPyProbe : Attribute{} // Create a custom attribute that should be placed on any value that you want to be plotted.

		public static UPyPlotController instance;

		void Awake () 
		{
			if (instance != null) 
			{ // Singleton pattern.
				Destroy(gameObject);
			}else{
				instance = this;
			}

			string plotDir = Application.dataPath + "/UPyPlot/plotting_cache/"; // The directory to create the plot data file in.
			if (!Directory.Exists(plotDir))
			{
				Directory.CreateDirectory(plotDir);
			}
			absoluteName = plotDir + plotFileName;

			CacheProbes ();
		}

		void OnEnable() 
		{
			Invoke ("CheckProbes", 0);
		}
		void OnDisable() 
		{
			CancelInvoke ("CheckProbes");
		}

		void CheckProbes () 
		{
			if (probes.Count > 0) {
				currentSample++;

				string line = ""; // The string that will be written into the plot data file.
				for (int i = 0; i < probes.Count; i++) {
					FieldInfo fieldInfo = probes [i];
					MonoBehaviour mono = monos [i];
					line += ((float)fieldInfo.GetValue (mono)).ToString ("F" + precision);
					if (i < probes.Count - 1) {
						line += ','; // Add a delimeter after all but the last index.
					}
				}
		
				List<String> lines = new List<String> (File.ReadAllLines (absoluteName));

				if (currentSample >= maxSamples) { // Handle rolling the file when max number of lines has been reached.
					int range = currentSample - maxSamples; // If slider was adjust by more than one, this will be the range.
					lines.RemoveRange (2, range); // Dont modify the first two lines (meta data, header data) but remove all lines for range after them.
					currentSample -= range; // Update the current sample count to accuratly reflect the number of line now currently in the plot data file.
				}
				lines.Add (line); // Add the new plot data string to the lines list.
				lines [0] = currentSample + "," + Time.time.ToString ("F2"); // Update the plot file meta data so the Python plot knows correct number of samples and gets the proper gametime for x axis.
				File.WriteAllLines (absoluteName, lines.ToArray ()); // Write all the data to the file.
			}
			Invoke ("CheckProbes", interval); // Start next cycle after interval delay.
		}

		private void CacheProbes() 
		{
			/*
			 * Rip through all gameobjects using reflection and look for any field with 
			 * custom attributes, if a "[UPyProbe]" attribute is found, then cache the 
			 * fieldinfo and monobehavior so it can be polled at regular intervals later.
			*/

			object[] obj = GameObject.FindSceneObjectsOfType(typeof (GameObject)); // All gameobjects in the scene.
			foreach (object o in obj)
			{
				GameObject g = (GameObject) o; 
				object[] ms = g.GetComponents (typeof(MonoBehaviour)); // The list of monobehavior scripts for this gameobject.
				foreach (MonoBehaviour m in ms) 
				{
					const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance; // Flags to filter the list of fields that are checked for custom attributs.
					try {
						FieldInfo[] fields = m.GetType().GetFields(flags); 
						foreach (FieldInfo fieldInfo in fields) // For each field in this monobehavior check it for custom attributs
						{
							var attrs = fieldInfo.GetCustomAttributes(typeof(UPyProbe), false); // Check this field for only [UPyProbe] custom attributs, ignore any others.
							var hasProbe = attrs.Length > 0; // True if one was present.
							if (hasProbe) // the field in this monobehavior had a "[UPyProbe]" Custom attribute.
							{ 
								monos.Add (m); // Cache the monobehavior script for this field.
								probes.Add (fieldInfo); // Cahce the fieldInfo.
								//Debug.Log ("Obj: " + m.name + "|" + m.GetType().Name + ", Field: " + fieldInfo.Name + ", Type: " + fieldInfo.GetValue(m) ) ;
							}
						}
					} catch {

					}
				}
			}
			// done caching, time to start building the plot data file.
			CreateFileHeader ();
		}

		private void CreateFileHeader() 
		{
			/*
			 * If file already exists then this clears it's contents and prepares it for the next cycle.
			 * It then creates a header on the first line containing all of the cached fiels names in order.
			 */
			string line = ""; // The string that will be written into the plot data file.
			for (int i = 0; i < probes.Count; i++) 
			{
				FieldInfo fieldInfo = probes[i];
				MonoBehaviour mono = monos [i];
				line += mono.name + "\\" + mono.GetType().Name + "\\" + fieldInfo.Name;
				if (i < probes.Count-1) 
				{
					line += ','; // Add a delimeter after all but the last index.
				}
			}
			File.WriteAllText(absoluteName, '\n' + line + '\n');
		}
	}
}