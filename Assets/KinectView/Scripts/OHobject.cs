using UnityEngine;
using System.Collections;
using System.Net;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

public class OHobject : MonoBehaviour
{

    const float HOVER_TIMER = 0.5f;

    private String itemName = "item";
    private bool selected = false;
    private GameObject inputFieldGo;

    float hoverTimer;
    bool hover = false;

    bool state = false;

    public bool Selected
    {
        get
        {
            return this.selected;
        }
        set
        {
            selected = value;
            if (!selected)
            {
                removeUI();
            }
            updateSelected();
        }
    }

    public string ItemName
    {
        get
        {
            return this.itemName;
        }
        set
        {
            itemName = value;
        }
    }

    // Use this for initialization
    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        rend.material.color = Color.blue;
    }

    /*void OnMouseDrag()
    {
        float distance_to_screen = Camera.current.ScreenPointToRay(gameObject.transform.position).z;
        transform.position = Camera.current.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance_to_screen));
    }*/

    private float dist;
    private Vector3 v3Offset;
    private Plane plane;

    void OnMouseDown()
    {

        plane.SetNormalAndPosition(Camera.main.transform.forward, transform.position);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float dist;
        plane.Raycast(ray, out dist);
        v3Offset = transform.position - ray.GetPoint(dist);
    }

    void OnMouseDrag()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float dist;
        plane.Raycast(ray, out dist);
        Vector3 v3Pos = ray.GetPoint(dist);
        transform.position = v3Pos + v3Offset;
    }

    // Update is called once per frame
    void Update()
    {
        if (hover && Time.fixedTime - hoverTimer > HOVER_TIMER)
        {
            state = !state;
            NetworkHandler.getInstance().SendCommand(state ? "OFF" : "ON", itemName);

            hover = false;
        }

        if (selected)
        {
            Vector3 move = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
            transform.position += move * 2 * Time.deltaTime;

            if (Input.GetKeyDown("delete"))
            {
                DataStore.Instance.RemoveOhObject(this.gameObject);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        hover = true;
        hoverTimer = Time.fixedTime;

        Renderer rend = GetComponent<Renderer>();
        rend.material.color = Color.red;
    }

    void OnCollisionExit(Collision collision)
    {
        Renderer rend = GetComponent<Renderer>();
        rend.material.color = Color.blue;

        hover = false;
    }

    void OnMouseOver()
    {

        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            transform.position = transform.position + new Vector3(0, 0, Input.GetAxis("Mouse ScrollWheel")*10);
        }

        if (Input.GetMouseButtonDown(0))
        {
            bool isSelected = !selected;
            deselectAll();
            Selected = isSelected;

            Debug.Log("Object clicked " + itemName);

            if (isSelected)
            {
                inputFieldGo = GameObject.Find("ItemNameText");
                if (inputFieldGo == null)
                {
                    inputFieldGo = Instantiate(Resources.Load("ItemNameText")) as GameObject;
                    inputFieldGo.transform.position = new Vector3(100, 200, 0);
                    GameObject canvas = GameObject.Find("Canvas");
                    inputFieldGo.transform.SetParent(canvas.transform);
                }
                InputField inputFieldCo = inputFieldGo.GetComponent<InputField>();

                inputFieldCo.onValueChange.RemoveAllListeners();
                inputFieldCo.text = itemName;
                inputFieldCo.onValueChange.AddListener(delegate
                {
                    Debug.Log("OH text " + inputFieldCo.text);
                    itemName = inputFieldCo.text;
                    DataStore.Instance.AddOhObject(this.gameObject);
                });
            }
            else
            {
                removeUI();
            }

            DataStore.Instance.AddOhObject(this.gameObject);
            DataStore.Instance.Save();
        }
    }

    void updateSelected()
    {
        Renderer rend = GetComponent<Renderer>();
        rend.material.color = selected ? Color.green : Color.blue;
    }

    void removeUI()
    {
        if (inputFieldGo != null)
        {
            Destroy(inputFieldGo);
        }
    }

    public void OnDestroy()
    {
        removeUI();
    }

    static void deselectAll()
    {
        foreach (var gameObject in GameObject.FindGameObjectsWithTag("OHItem"))
        {
            OHobject ohObject = gameObject.GetComponent<OHobject>();
            if (ohObject != null)
            {
                ohObject.Selected = false;
            }
        }
    }
}
