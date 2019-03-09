using Unity.Engine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
// using <stdlib.h>;
// using <stdio.h>;
// To decide if the player will be a white blood cell or a random virus.

public class ModalPanel : MonoBehaviour {

    public Text question;
    public Image iconImage;
    public Button wbcButton;
    public Button virusButton;
    public Button randVirusButton;
    public Button cancelButton;
    public GameObject modalPanelObject;
    
    private static ModalPanel modalPanel;
    
    public static ModalPanel Instance () {
        if (!modalPanel) {
            modalPanel = FindObjectOfType(typeof (ModalPanel)) as ModalPanel;
            if (!modalPanel)
                Debug.LogError ("There needs to be one active ModalPanel script on a GameObject in your scene.");
        }
        
        return modalPanel;
    }

    // WBC/No/Cancel: A string, a WBC event, a Virus event, a random event, and Cancel event
    public void Choice (string question, UnityAction wbcEvent, UnityAction virusEvent, UnityAction randEvent, UnityAction cancelEvent) {
    //   private UnityAction randomEvent;
    //   private time_t t;
    //   srand((unsigned) time(&t));

      // insert a new random object to take the place of the 1 in the next line
      if (rand() % 2 == 1) {
          randEvent = wbcEvent;
      } else {
          randEvent = virusEvent;
      }
    //   randEvent = (rand() % 2 == 1) ? wbcEvent : virusEvent; // the randEvent is fixed to either white blood cell or a virus.
        modalPanelObject.SetActive (true);
        
        wbcButton.onClick.RemoveAllListeners();
        wbcButton.onClick.AddListener (wbcEvent);
        wbcButton.onClick.AddListener (ClosePanel);
        
        virusButton.onClick.RemoveAllListeners();
        virusButton.onClick.AddListener (virusEvent);
        virusButton.onClick.AddListener (ClosePanel);

        randVirusButton.onClick.RemoveAllListeners();
        randVirusButton.onClick.AddListener (randEvent);
        randVirusButton.onClick.AddListener (ClosePanel);
        
        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener (cancelEvent);
        cancelButton.onClick.AddListener (ClosePanel);

        this.question.text = question;

        this.iconImage.gameObject.SetActive (false);
        wbcButton.gameObject.SetActive (true);
        virusButtion.gameObject.SetActive(true);
        randVirusButton.gameObject.SetActive (true);
        cancelButton.gameObject.SetActive (true);
    }

    void ClosePanel () {
        modalPanelObject.SetActive (false);
    }
}
