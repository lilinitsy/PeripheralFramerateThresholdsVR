using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System;

[CreateAssetMenu(fileName = "ExperimentParams", menuName = "Scriptable Objects/ExperimentParams")]
public class ExperimentParams : ScriptableObject
{
	// Parameters should be set in the editor if they need to vary from default
	// Default is set up to work with ExperimentManagerTowards
	public int default_framerate = 180;
	public Vector3 camera_position = new Vector3(0.0f, 1.0f, 0.0f);

	public float[] eccentricities = {15.0f, 25.0f, 35.0f, 45.0f};
	private float[] x_positions; // determined from input eccentricities

	public float y_position = 1.0f;
	public float z_position = 25.0f;

	// Input as degrees for the rotation parameters
	public float[] speeds = {5.0f, 10.0f, 15.0f, 20.0f};

	public int[] alternate_framerates = {12, 18, 24, 30, 36, 45, 60, 90, 180};

	public List<TrialConfig> trials = new List<TrialConfig>();


	private void compute_x_positions()
	{
		x_positions = new float[eccentricities.Length];
		for(int i = 0; i < eccentricities.Length; i++)
		{
			float radians = eccentricities[i] * Mathf.Deg2Rad;
			x_positions[i] = z_position * Mathf.Tan(radians);
		}
	}

	public void generate_trials()
	{
		trials.Clear();
		compute_x_positions();

		for(int i = 0; i < x_positions.Length; i++)
		{
			float x = x_positions[i];
			float ecc = eccentricities[i];


			foreach (float speed in speeds)
			{
				foreach (int alt_framerate in alternate_framerates)
				{
					trials.Add(new TrialConfig
					{
						left_object_position = new Vector3(-x, y_position, z_position),
						right_object_position = new Vector3(x, y_position, z_position),
						eccentricity = ecc,
						speed = speed,
						left_object_frame_rate = default_framerate,
						right_object_frame_rate = alt_framerate
					});

					trials.Add(new TrialConfig
					{
						left_object_position = new Vector3(-x, y_position, z_position),
						right_object_position = new Vector3(x, y_position, z_position),
                        eccentricity = ecc,
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
	public float eccentricity; // absolute, doesn't indicate left/right
	public float speed;
	public int left_object_frame_rate;
	public int right_object_frame_rate;
}
