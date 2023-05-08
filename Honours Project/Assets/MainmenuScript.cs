using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine;

public class MainmenuScript : MonoBehaviour
{
    //Background song for main menu
    // Airtone, 2021, Precarity Available at: https://dig.ccmixter.org/files/airtone/64030 Acessecd on: 15/4/23
    // This work is licensed under Creative Commons Attribution-NonCommercial 3.0 License

    //Text on screen to display the players statistics
    [SerializeField]
   List< TMP_Text> death;
    [SerializeField]
    List<TMP_Text> score;
    [SerializeField]
    List<TMP_Text> accuracy;
    //timer to stop clicking of a level, added as mulit click skipped the main menu
    private float timer;
    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        //check if the first level is complete
        if (DataStorage.level1com)
        {
            //if complete update the statistics
            death[0].text = "Deaths level 1: " + DataStorage.level1Death.ToString();
            score[0].text = "Score in level 1: " + DataStorage.level1Score.ToString();
                     
            accuracy[0].text = "Accuracy in level 1: " + DataStorage.accuracy1.ToString() + "%";
            Debug.Log(DataStorage.accuracy1);
        }
        //check if the second level is complete
        if (DataStorage.level2com)
        {
            //if so update the statistics
            death[1].text = "Deaths level 2: " + DataStorage.level2Death.ToString();
            score[1].text = "Score in level 2: " + DataStorage.level2Score.ToString();
                    
            accuracy[1].text = "Accuracy in level 2: " + DataStorage.accuracy2.ToString() + "%";
        }
        //check if the third level is complete 
        if (DataStorage.level3com)
        {
            //if so update statistics
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
    //load the level based on the string given
    public void loadScene(string screenName)
    {
        if (timer >= 2)
        {
            SceneManager.LoadScene(screenName);
        }
        

       
           
    }
}
