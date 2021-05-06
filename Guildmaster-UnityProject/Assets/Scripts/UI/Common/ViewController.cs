using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public struct ViewParameters {
    public string viewName;
    public string viewSource;
}

public class ViewController : MonoBehaviour
{
    /*
     Base class for all views
    */

    protected ViewManager viewManager; //used to boot up views
    protected string viewName;
    public string ViewName   // property
    {
        get { return viewName; }   // get method
    }    
    protected string source; //tells you from where the view was called
    public string Source   // property
    {
        get { return source; }   // get method
        set { source = value; }  // set method
    }

    protected virtual void Intialize()
    {

    }

    protected virtual async Task IntializeAsync() 
    {

    }

    void OnEnable() 
    {
        OnShown();
        OnShownAsync();
    }

    protected virtual void OnShown()
    {

    }

    protected virtual async Task OnShownAsync()
    {

    }

    void OnDisable()
    {
        OnHidden();
        OnHiddenAsync();
    }

    protected virtual void OnHidden()
    {

    }

    protected virtual async Task OnHiddenAsync()
    {

    }

    protected virtual void Refresh()
    {

    }

    protected virtual async Task RefreshAsync()
    {

    }

    private void MakeDirty()
    {
        Refresh();
        RefreshAsync();
    }

    private void Back() //go back to source view
    {
        Debug.Log(ViewName + "closed, Go back to " + Source);
        //close view
        gameObject.SetActive(false); 
        //open the source view
        if(string.IsNullOrEmpty(Source))
        {
            viewManager.ShowView(new ViewParameters{ viewName = Source, viewSource = "" });
        }
    }



}
