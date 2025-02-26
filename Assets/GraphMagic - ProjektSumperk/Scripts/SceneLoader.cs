using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjektSumperk
{
    public class SceneLoader : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SceneManager.LoadScene(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SceneManager.LoadScene(2);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SceneManager.LoadScene(3);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                SceneManager.LoadScene(4);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                SceneManager.LoadScene(5);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                SceneManager.LoadScene(6);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                SceneManager.LoadScene(7);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                SceneManager.LoadScene(8);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                SceneManager.LoadScene(9);
            }

            else if (Input.GetKeyDown(KeyCode.A))
            {
                SceneManager.LoadScene(10);
            }

            else if (Input.GetKeyDown(KeyCode.B))
            {
                SceneManager.LoadScene(11);
            }

            else if (Input.GetKeyDown(KeyCode.C))
            {
                SceneManager.LoadScene(12);
            }

            else if (Input.GetKeyDown(KeyCode.D))
            {
                SceneManager.LoadScene(13);
            }

            else if (Input.GetKeyDown(KeyCode.E))
            {
                SceneManager.LoadScene(14);
            }
        }
    }

}

