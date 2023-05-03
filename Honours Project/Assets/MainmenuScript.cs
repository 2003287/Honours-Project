using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine;

public class MainmenuScript : MonoBehaviour
{
    [SerializeField]
   List< TMP_Text> death;
    [SerializeField]
    List<TMP_Text> score;
    [SerializeField]
    List<TMP_Text> accuracy;
    private float timer;
    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        if (DataStorage.level1com)
        {
            death[0].text = "Deaths level 1: " + DataStorage.level1Death.ToString();
            score[0].text = "Score in level 1: " + DataStorage.level1Score.ToString();
                     
            accuracy[0].text = "Accuracy in level 1: " + DataStorage.accuracy1.ToString() + "%";
            Debug.Log(DataStorage.accuracy1);
        }
        if (DataStorage.level2com)
        {
            death[1].text = "Deaths level 2: " + DataStorage.level2Death.ToString();
            score[1].text = "Score in level 2: " + DataStorage.level2Score.ToString();
                    
            accuracy[1].text = "Accuracy in level 2: " + DataStorage.accuracy2.ToString() + "%";
        }
        if (DataStorage.level3com)
        {
            death[2].text = "Deaths in level 3: " + DataStorage.level3Death.ToString();
            score[2].text = "Score in level 3: " + DataStorage.level3Score.ToString();
            
            accuracy[2].text = "Accuracy in level 3: " + DataStorage.accuracy3.ToString() + "%";
        }
        timer = 0;
        Debug.Log("level completesapwn");

    }
    private void Update()
    {
        timer += Time.deltaTime;
        
    }
    public void loadScene(string screenName)
    {
        if (timer >= 2)
        {
            SceneManager.LoadScene(screenName);
        }
        

       
           
    }
}
