using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChessboardTile : MonoBehaviour
{
    public int row; // Sat�r numaras�
    public int col; // S�tun numaras�

    // Kare konumunu ayarlamak i�in kullan�lan fonksiyon
    public void SetPosition(int rowIndex, int colIndex)
    {
        row = rowIndex;
        col = colIndex;

        // Kare �zerinde say�y� ve harfi g�steren metni g�ncelleme
        Transform numberTextTransform = transform.Find("numberTextMesh");
        Transform letterTextTransform = transform.Find("letterTextMesh");

        if (row == 0)
        {
            // Sat�r 0 ise, say� metnini g�ncelle

            // Say� metnini bul
            TextMeshPro numberTextMesh = numberTextTransform.transform.GetComponent<TextMeshPro>();
            if (numberTextMesh != null)
            {
                numberTextMesh.text = (col + 1).ToString();
            }

            // Kare rengine g�re metin rengini ayarlama
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
            // Sat�r 0 de�ilse, say� metnini sil
            Destroy(numberTextTransform.gameObject);
        }

        if (col == 0)
        {
            // S�tun 0 ise, harf metnini g�ncelle

            // Harf metnini bul
            TextMeshPro letterTextMesh = letterTextTransform.transform.GetComponent<TextMeshPro>();
            if (letterTextMesh != null)
            {
                letterTextMesh.text = ((char)('a' + row)).ToString();
            }

            // Kare rengine g�re metin rengini ayarlama
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
            // S�tun 0 de�ilse, harf metnini sil
            Destroy(letterTextTransform.gameObject);
        }
    }
}