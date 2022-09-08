
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PoolObject : MonoBehaviour, IComparable<PoolObject>
{
    /// <summary>
    /// the id of the object
    /// </summary>
    protected string ID;

    /// <summary>
    /// The tag type of the object
    /// </summary>
    /// <value></value>
    public string Tag { get; protected set; }

    /// <summary>
    /// handles issues when moving the character after re-enabling it
    /// </summary>
    private CharacterController cc;


    /// <summary>
    /// Set the new object
    /// </summary>
    /// <param name="tag"></param>
    public void Init(string tag)
    {
        if (tag == null)
            throw new NullReferenceException("the tag cannot be null");

        if (tag == string.Empty)
            throw new ArgumentNullException("the tag cannot be empty");

        ID = Guid.NewGuid().ToString();
        Tag = tag;

        cc = GetComponent<CharacterController>();

    }

    /// <summary>
    /// enable the object   
    /// </summary>
    public void Enable()
    {

        if (cc != null)
            cc.enabled = false;

        this.gameObject.SetActive(true);
    }

    /// <summary>
    /// set the position of the object and enable it
    /// </summary>
    /// <param name="position">the position</param>
    public void Enable(Vector3 position)
    {
        this.transform.position = position;
        Enable();
    }

    /// <summary>
    /// Get the tag of the object
    /// </summary>
    /// <returns>returns the tag as a string</returns>
    public string GetTag()
    {
        return Tag;
    }

    public int CompareTo(PoolObject other)
    {
        if (other == null || other.ID == null)
            return this.ID.CompareTo("");

        return this.ID.CompareTo(other.ID);
    }

    public override string ToString()
    {
        return this.gameObject + " id: " + ID + ")";
    }

    /// <summary>
    /// disable the object
    /// </summary>
    public void Disable()
    {
        if (cc != null)
            cc.enabled = false;

        this.gameObject.SetActive(false);
    }
}
