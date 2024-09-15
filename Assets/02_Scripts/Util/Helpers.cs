using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

public static class Helpers
{
    #region Helper Functions

    private static readonly Dictionary<float, WaitForSeconds> WaitDictionary = new Dictionary<float, WaitForSeconds>();
    /// <summary>
    /// GetWait metodu ihtiyaç duyduğumuz miktarda zaman parametresine sahip WaitForSeconds'ı yaratarak bize döner.
    /// Yaratılan süredeki WaitForSeconds bir dictionary içerisine kaydedilir.
    ///
    /// Bir coroutine içerisinde defalarca WaitForSeconds yield edilmesi gerektiğinde her defasında aynı süreye sahip
    /// yeni WaitForSeconds'lar yaratılmaz. Sözlükte hazır bekletilen dönülür.
    ///
    /// Bu sayede garbage allocation azaltılır.
    /// </summary>
    /// <param name="time"></param>
    /// Fonksiyonun döneceği WaitForSeconds'ın sahip olduğu süre.
    /// <returns></returns>
    /// 
    public static WaitForSeconds GetWait(float time)
    {
        if (WaitDictionary.TryGetValue(time, out var wait)) return wait;

        WaitDictionary[time] = new WaitForSeconds(time);
        return WaitDictionary[time];
    }

    private static PointerEventData eventDataCurrentPosition;
    private static List<RaycastResult> results;

    
    /// <summary>
    /// Bu method anlık olarak mousePosition'a ray atarak, pointer'ın o an bir UI elemanı üzerinde olup olmadığının
    /// kontrolünü yapar.
    ///
    /// Eğer fare imleci UI üzerindeyse True döner.
    /// </summary>
    /// <returns></returns>
    public static bool IsOverUI()
    {
        eventDataCurrentPosition = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    
    /// <summary>
    /// Kendisine verilen RectTransform ve Camera aracılığıyla RectTransform Canvas elemanının World Position'ını döner.
    /// </summary>
    /// <param name="element"></param>
    /// <param name="camera"></param>
    /// <returns></returns>
    public static Vector2 GetWorldPositionOfCanvasElement(RectTransform element, Camera camera)
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(element, element.position, camera, out var result);
        return result;
    }
    
    /// <summary>
    /// Bir noktanın bir doğruya en yakın olduğu (doğrunun üzerindeki) noktayı döner.
    /// </summary>
    /// <param name="lineStart"></param>
    /// Doğrunun başlangıç pozisyonu
    /// <param name="lineEnd"></param>
    /// Doğrunun bitiş pozisyonu
    /// <returns></returns>
    public static Vector3 NearestPoint(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
    {
        Vector3 lineDirection = Vector3.Normalize(lineEnd - lineStart);
        float closestPoint = Vector3.Dot((point - lineStart), lineDirection) / Vector3.Dot(lineDirection, lineDirection);
        return lineStart + (closestPoint * lineDirection);
    }

    #endregion

    #region Transform Extensions
    
    /// <summary>
    /// Runtime'da kendisine verilen transformun bütün çocuklarını destroy eder.
    /// </summary>
    /// <param name="t"></param>
    public static void DestroyAllChildren(this Transform t)
    {
        foreach (Transform child in t) Object.Destroy(child.gameObject);
    }
    
    /// <summary>
    /// Editör içerisindeyken (oyun oynanmıyorken) kendisine verilen tranformun bütün çocuklarını destroy eder. 
    /// </summary>
    /// <param name="t"></param>
    public static void DestroyAllChildrenImmediate(this Transform t)
    {
        foreach (Transform child in t) Object.DestroyImmediate(child.gameObject);  
    }
    
    #endregion

    #region Collection Extensions
    
    /// <summary>
    /// Kendisine verilen listenin son elemanını listeden çıkararak döndürür. (Stack -> Pop())
    /// </summary>
    /// <param name="t"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T Pop<T>(this List<T> t)
    {
        T lastItem = t[^1];
        t.Remove(lastItem);
        return lastItem;
    }
    
    /// <summary>
    /// Kendisine verilen listenin ilk elemanını listeden çıkararak döndürür. (Queue -> Dequque())
    /// </summary>
    /// <param name="t"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T Dequeue<T>(this List<T> t)
    {
        T firstItem = t[0];
        t.Remove(firstItem);
        return firstItem;
    }

    /// <summary>
    /// Listeden rastgele bir eleman seçerek döndürür. True parametresi verilirse döndürdüğü elemanı listeden silerek
    /// döndürür.
    /// </summary>
    /// <param name="t"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetRandom<T>(this List<T> t) { return t[Random.Range(0, t.Count)]; }

    /// <summary>
    /// Listeden rastgele bir eleman seçerek döndürür, true parametresi verilirse döndürdüğü elemanı listeden silerek
    /// döndürür.
    /// </summary>
    /// <param name="t"></param>
    /// <param name="removeFromList"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetRandom<T>(this List<T> t, bool removeFromList)
    {
        T randomItem = GetRandom(t);
        if (removeFromList) t.Remove(randomItem);
        return randomItem;
    }

    #endregion

    #region Vector Extensions

    public static Vector2 SetX(this Vector2 vector, float x)
    {
        return new Vector2(x, vector.y);
    }
    
    public static Vector2 AddX(this Vector2 vector, float x)
    {
        return new Vector2(vector.x + x, vector.y);
    }
    
    public static Vector3 SetX(this Vector3 vector, float x)
    {
        return new Vector3(x, vector.y, vector.z);
    }
    
    public static Vector3 AddX(this Vector3 vector, float x)
    {
        return new Vector3(vector.x + x, vector.y, vector.z);
    }
    
    public static Vector3 SetY(this Vector3 vector, float y)
    {
        return new Vector3(vector.x, y, vector.z);
    }
    
    public static Vector3 AddY(this Vector3 vector, float y)
    {
        return new Vector3(vector.x, vector.y+y, vector.z);
    }
    
    public static Vector3 SetZ(this Vector3 vector, float z)
    {
        return new Vector3(vector.x, vector.y, z);
    }

    public static Vector3 AddZ(this Vector3 vector, float z)
    {
        return new Vector3(vector.x, vector.y, vector.z + z);
    }

    #endregion

    #region Rigidbody Extensions

    /// <summary>
    /// Changes the direction of a rigidbody without changing its speed.
    /// </summary>
    /// <param name="rigidbody">Rigidbody.</param>
    /// <param name="direction">New direction.</param>
    public static void ChangeDirection(this Rigidbody rigidbody, Vector3 direction)
    {
        rigidbody.velocity = direction * rigidbody.velocity.magnitude;
    }

    #endregion
}