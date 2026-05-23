using UnityEngine;
using System.Collections;

public class PizzaItem : MonoBehaviour
{
    public int pizzaType = 1;
    public float animDuration = 0.25f;

    [HideInInspector] public Slot mySlot;
    [HideInInspector] public PlateItem myPlate;
    [HideInInspector] public bool isMoving = false;

    public void MoveTo(int rotationIndex)
    {
        StopAllCoroutines();
        StartCoroutine(AnimateToPosition(rotationIndex));
    }

    public void SnapTo(int rotationIndex)
    {
        StopAllCoroutines();
        isMoving = false;
        transform.localPosition = new Vector3(0, 0.7f, 0);
        transform.localRotation = Quaternion.Euler(0, rotationIndex * 60f, 0);
    }

    private IEnumerator AnimateToPosition(int rotationIndex)
    {
        isMoving = true;

        Vector3 startPos = transform.localPosition;
        Quaternion startRot = transform.localRotation;
        Vector3 endPos = new Vector3(0, 0.7f, 0);
        Quaternion endRot = Quaternion.Euler(0, rotationIndex * 60f, 0);

        float elapsed = 0f;
        while (elapsed < animDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / animDuration));
            transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            transform.localRotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }

        transform.localPosition = endPos;
        transform.localRotation = endRot;
        isMoving = false;
    }
}