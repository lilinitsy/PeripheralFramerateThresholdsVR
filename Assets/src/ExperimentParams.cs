using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ExperimentParams", menuName = "Scriptable Objects/ExperimentParams")]
public class ExperimentParams : ScriptableObject
{
	// Parameters should be set in the editor if they need to vary from default
	// Default is set up to work with ExperimentManagerTowards
	public int default_framerate = 120;
	public Vector3 camera_position = new Vector3(0.0f, 1.0f, 0.0f);

	public float[] x_positions = {12.0f, 10.0f, 8.0f, 6.0f, 4.0f, 2.0f};
	public float y_position = 1.0f;
	public float z_position = 25.0f;

	public float[] speeds = {5.0f, 10.0f, 15.0f, 20.0f};

	public int[] alternate_framerates = {12, 15, 20, 30, 40, 60, 120};

	public List<TrialConfig> trials = new List<TrialConfig>();

	public void generate_trials()
	{
		trials.Clear();

		foreach (float x in x_positions)
		{
			//float z = Mathf.Sqrt(10f - x * x); // This is causing NAN's wtf, fix it later.

			foreach (float speed in speeds)
			{
				foreach (int alt_framerate in alternate_framerates)
				{
					trials.Add(new TrialConfig
					{
						left_object_position = new Vector3(-x, y_position, z_position),
						right_object_position = new Vector3(x, y_position, z_position),
						speed = speed,
						left_object_frame_rate = default_framerate,
						right_object_frame_rate = alt_framerate
					});

					trials.Add(new TrialConfig
					{
						left_object_position = new Vector3(-x, y_position, z_position),
						right_object_position = new Vector3(x, y_position, z_position),
						speed = speed,
						left_object_frame_rate = alt_framerate,
						right_object_frame_rate = default_framerate
					});
				}
			}
		}
	}
}

[System.Serializable]
public class TrialConfig
{
	public Vector3 left_object_position; // top
	public Vector3 right_object_position; // bottom
	public float speed;
	public int left_object_frame_rate;
	public int right_object_frame_rate;
}
