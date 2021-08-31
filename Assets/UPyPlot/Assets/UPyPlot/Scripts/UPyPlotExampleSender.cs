using UnityEngine;

public class UPyPlotExampleSender : MonoBehaviour {

	[UPyPlot.UPyPlotController.UPyProbe] // Add probe so this value will be plotted.
	private float xVar;

	[UPyPlot.UPyPlotController.UPyProbe] // Add probe so this value will be plotted.
	private float zVar; 

	[UPyPlot.UPyPlotController.UPyProbe] // Add probe so this value will be plotted.
	[Range(-10,10)]                      // Add a manual control slider so its easy to change in real time.
	[SerializeField] public float yVar;  // A public variable that user can adjust manually and see in plot.

	private float lastRndX = 0;
	private float lastRndZ = 0;

	void Update () { // Some example code that makes the values change in the plot.
		xVar = Mathf.Lerp(lastRndX, Random.Range (0.0f, 10.0f), Time.deltaTime * 0.5f );
		lastRndX = xVar;

		zVar = Mathf.Lerp(lastRndZ, Random.Range(-10.0f, 10.0f), Time.deltaTime * 0.5f );
		lastRndZ = zVar;

		transform.position = new Vector3 (xVar, yVar, zVar); // Move the example sphere.
	}
}
