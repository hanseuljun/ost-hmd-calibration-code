using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class CalibrationFile {

	private static readonly string fileName = "Calibration.txt";
	public List<Vector3> targets { get; private set; }
	public List<Vector3> fingerTips { get; private set; }
	private StreamWriter sw;

	public CalibrationFile() {
		targets = new List<Vector3> ();
		fingerTips = new List<Vector3> ();

		if (File.Exists (fileName)) {
			StreamReader sr = File.OpenText(fileName);
			string line = sr.ReadLine();
			while(line != null) {
				string[] words = line.Split(' ');
				if(words.Length == 6) {
					Vector3 target = new Vector3(float.Parse(words[0]), float.Parse(words[1]), float.Parse(words[2]));
					targets.Add(target);
					Vector3 fingerTip = new Vector3(float.Parse(words[3]), float.Parse(words[4]), float.Parse(words[5]));
					fingerTips.Add(fingerTip);
				}
				line = sr.ReadLine();
			}
			sr.Close();

			sw = File.AppendText (fileName);
		}
		else {
			sw = File.CreateText(fileName);
		}
	}

	public void AddFingerTip(Vector3 target, Vector3 fingerTip) {
		fingerTips.Add (target);
		sw.Write (string.Format ("{0} {1} {2} {3} {4} {5}\n", target.x, target.y, target.z,
		                         fingerTip.x, fingerTip.y, fingerTip.z));
	}

	public void Write(string str) {
		sw.Write (str);
	}
	
	public void Close() {
		sw.Close ();
	}
}
