using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class ViewManager : MonoBehaviour
{
    /*
     Do not destroy object.
     */

    [SerializeField] List<ViewController> views; 

    public void ShowView(ViewParameters viewParameters)
    { 
        //might have to change the way its structure these
        foreach(ViewController view in views){
            if(viewParameters.viewName.Equals(view.ViewName))
            {
                if(!string.IsNullOrEmpty(viewParameters.viewSource))
                {
                    view.Source = viewParameters.viewSource;
                }
                view.gameObject.SetActive(true); 
                return;
            }
        }
    }




}
