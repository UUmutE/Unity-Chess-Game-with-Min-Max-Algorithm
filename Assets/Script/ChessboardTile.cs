using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChessboardTile : MonoBehaviour
{
    public int row; // Satýr numarasý
    public int col; // Sütun numarasý

    // Kare konumunu ayarlamak için kullanýlan fonksiyon
    public void SetPosition(int rowIndex, int colIndex)
    {
        row = rowIndex;
        col = colIndex;

        // Kare üzerinde sayýyý ve harfi gösteren metni güncelleme
        Transform numberTextTransform = transform.Find("numberTextMesh");
        Transform letterTextTransform = transform.Find("letterTextMesh");

        if (row == 0)
        {
            // Satýr 0 ise, sayý metnini güncelle

            // Sayý metnini bul
            TextMeshPro numberTextMesh = numberTextTransform.transform.GetComponent<TextMeshPro>();
            if (numberTextMesh != null)
            {
                numberTextMesh.text = (col + 1).ToString();
            }

            // Kare rengine göre metin rengini ayarlama
            if (transform.GetComponent<Renderer>().material.color == Color.black)
            {
                numberTextMesh.color = Color.white;
            }
            else
            {
                numberTextMesh.color = Color.black;
            }
        }
        else
        {
            // Satýr 0 deðilse, sayý metnini sil
            Destroy(numberTextTransform.gameObject);
        }

        if (col == 0)
        {
            // Sütun 0 ise, harf metnini güncelle

            // Harf metnini bul
            TextMeshPro letterTextMesh = letterTextTransform.transform.GetComponent<TextMeshPro>();
            if (letterTextMesh != null)
            {
                letterTextMesh.text = ((char)('a' + row)).ToString();
            }

            // Kare rengine göre metin rengini ayarlama
            if (transform.GetComponent<Renderer>().material.color == Color.black)
            {
                letterTextMesh.color = Color.white;
            }
            else
            {
                letterTextMesh.color = Color.black;
            }
        }
        else
        {
            // Sütun 0 deðilse, harf metnini sil
            Destroy(letterTextTransform.gameObject);
        }
    }
}