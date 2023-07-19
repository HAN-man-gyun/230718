using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileMapController : MonoBehaviour
{
    protected string tileMapObjName = default;

    protected MapBoard mapController = default;
    protected Tilemap tileMap = default;
    protected List<GameObject> allTileObjs = default;

    //Awake타임에 초기화할 내용을 상속받은 클래스별로 재정의한다.

    public virtual void InitAwake(MapBoard mapController_)
    {
        mapController = mapController_;
        tileMap = gameObject.FindChildComponent<Tilemap>(tileMapObjName);

        allTileObjs = tileMap.gameObject.GetChildrenObjs();
        if(allTileObjs.IsValid())
        {
            allTileObjs.Sort(GFunc.CompareTileObjToLocalPos2D);
        }
        else
        {
            { allTileObjs = new List<GameObject>(); }
        }
    }
    // Start is called before the first frame update
    
}
