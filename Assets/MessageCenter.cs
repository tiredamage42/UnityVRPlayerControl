using System.Collections.Generic;
using UnityEngine;

public class MessageCenter : MonoBehaviour
{
    public static void DisplayMessage (string message, Color color, float duration, float fadeIn, float fadeOut) {
        if (instances.Count == 0) {
            Debug.LogError("No message center instances in the scene");
        }

        foreach (var instance in instances) {
            instance.ShowMessage(message, color, duration, fadeIn, fadeOut);
        }
    }
    static HashSet<MessageCenter> instances = new HashSet<MessageCenter>();

    public static void RemoveInstance (MessageCenter messageCenter) {
        instances.Remove(messageCenter);
    }
    public static void AddInstance (MessageCenter messageCenter) {
        instances.Add(messageCenter);
    }

    Queue<MessageElement> elementPool = new Queue<MessageElement>();
    List<MessageElement> shownElements = new List<MessageElement>();
    public MessageElement messagePrefab;

    public TextAlignment textAlignment;
    public TextAnchor textAnchor;
    public float elementMoveSpeed = 5;


    [Header("negative values flip directions")]
    public float startXOffset = .1f;
    public float lineHeight = .5f;

    public void ShowMessage (string message, Color color, float duration, float fadeIn, float fadeOut) {
        MessageElement newMessage = GetAvailableElement();
        newMessage.ShowMessage (message, color, duration, fadeIn, fadeOut, textAlignment, textAnchor);
        shownElements.Add(newMessage);
        newMessage.transform.localPosition = new Vector3(startXOffset, (shownElements.Count - 1) * lineHeight, 0);
    }

    public void DisableAllMessages () {
        for (int i =0; i < shownElements.Count; i++) {
            shownElements[i].DisableMessage();
            elementPool.Enqueue(shownElements[i]);
        }
        shownElements.Clear();
    }
    MessageElement BuildNewElement () {
        MessageElement newElement = Instantiate(messagePrefab);
        newElement.transform.SetParent(transform);
        newElement.transform.localRotation = Quaternion.identity;
        newElement.DisableMessage();
        return newElement;
    }

    MessageElement GetAvailableElement () {
        if (elementPool.Count > 0) 
            return elementPool.Dequeue();
        return BuildNewElement();
    }


    void UpdateShownElements (float deltaTime) {

        float speed = deltaTime * elementMoveSpeed;
        for (int i = 0; i < shownElements.Count; i++) {
            MessageElement element = shownElements[i];
            element.transform.localPosition = Vector3.Lerp(element.transform.localPosition, new Vector3 (0, i * lineHeight, 0), speed);
            element.UpdateElement(deltaTime);
        }

        for (int i = shownElements.Count - 1; i >= 0; i--) {
            MessageElement element = shownElements[i];
            if (element.isAvailable) {
                shownElements.Remove(element);
                elementPool.Enqueue(element);
            }
        }
    }

    void Update()
    {
        UpdateShownElements(Time.deltaTime);
    }
}
