using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SystemControl : MonoBehaviour {

	[SerializeField]
	protected GameObject questionCanvas;

    [SerializeField]
    protected string[] questions;

    [SerializeField]
    protected Text questionText;

    private int count = 0;

	// Use this for initialization
	void Start () {
        questionText.text = questions[count];
        questionCanvas.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.Escape))
			Application.Quit();
		
		if (Input.GetKeyDown (KeyCode.R)) {
			Scene scene = SceneManager.GetActiveScene ();
			SceneManager.LoadScene (scene.name);
		}
		if (Input.GetKeyDown (KeyCode.F)) {
			questionCanvas.SetActive (!questionCanvas.activeSelf);
		}
        if( Input.GetKeyDown(KeyCode.Q))
        {
            if(count <= questions.Length)
            {
                count++;
            }
            else { count = 0; }
            questionText.text = questions[count];
        }

	}
}
