using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class NetworkController : MonoBehaviour
{
    [SerializeField] private Button serverButton;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;


    private void Awake()
    {
        serverButton.onClick.AddListener(() => {
            NetworkManager.Singleton.StartServer();
            SceneManager.LoadScene("SampleScene");
        });

        hostButton.onClick.AddListener(() => {
            NetworkManager.Singleton.StartHost();
            SceneManager.LoadScene("SampleScene");
        });

        clientButton.onClick.AddListener(() => {
            NetworkManager.Singleton.StartClient();
            SceneManager.LoadScene("SampleScene");
        });
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
