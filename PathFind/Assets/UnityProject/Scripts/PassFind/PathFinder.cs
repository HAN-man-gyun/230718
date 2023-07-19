using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PathFinder : GSingleton<PathFinder>
{
    #region 지형탐색을 위한 변수
    public GameObject sourceObj = default;
    public GameObject destinationObj = default;
    public MapBoard mapBoard = default;
    #endregion //지형탐색을 위한변수

    #region A star 알고리즘으로 최단거리를 찾기 위한 변수
    private List<AstarNode> aStarResultPath = default;
    private List<AstarNode> aStarOpenPath = default;
    private List<AstarNode> aStarClosePath = default;
    #endregion //A star 알고리즘으로 최단거리를 찾기위한 변수

    //! 출발지와 목적지 정보로 길을 찾는 함수
    public void FindPath_Astar()
    {
        StartCoroutine(DelayFindPath_Astar(1.0f));
    } //FindPath_Astar()
    //!탐색 알고리즘에 딜레이를 건다
    private IEnumerator DelayFindPath_Astar(float delay_)
    {
        
        // A star 알고리즘을 사용하기 위해서 Path list를 초기화한다.
        aStarOpenPath = new List<AstarNode>();
        aStarClosePath = new List<AstarNode>();
        aStarResultPath = new List<AstarNode> ();

        TerrainController targetTerrain = default;

        //출발지의 인덱스를 구해서 출발지 노드를 찾아온다.
        string[] sourceObjNameParts = sourceObj.name.Split('_');
        int sourceIdx1D = -1;
        int.TryParse(sourceObjNameParts[sourceObjNameParts.Length - 1], out sourceIdx1D);
        targetTerrain = mapBoard.GetTerrain(sourceIdx1D);
        //찾아온 출발지 노드를 open리스트에 추가한다.
        AstarNode targetNode = new AstarNode(targetTerrain, destinationObj);
        Add_AstarOpenList(targetNode);

        int loopIdx = 0;
        bool isFoundDestination = false;
        bool isNowayToGo = false;
        //Todo : 알고리즘 정상작동 확인후 조건문 수정할 예정
        while(loopIdx<10)
        {
            // Open 리스트를 순회해서 가장 코스트가 낮은 노드를 선택한다.
            AstarNode minCostNode = default;
            foreach(var terrainNode in aStarOpenPath)
            {
                if(minCostNode == default)
                {
                    minCostNode = terrainNode;
                    //if: 가장 작은 코스트의 노드가  비어있는경우
                }
                else
                {
                    //terrainNode가 더 작은 코스트를 가지는경우
                    //minCostNode를 업데이트한다.
                    if (terrainNode.AstarF < minCostNode.AstarF)
                    {
                        minCostNode = terrainNode;
                    }
                    else { continue; }
                }   // else : 가장작은 코스트의 노드가 캐싱되어있는경우

            }   // Loop가장 코스트가 낮은 노드를 찾는 루프

            // Open 리스트를 순회해서 가장 코스트가 낮은 노드를 선택한다.
            minCostNode.ShowCost_Astar();
            minCostNode.Terrain.SetTileActiveColor(RDefine.TileStatusColor.SEARCH);

            // 선택한 노드가 목적지에 도달했는지 확인한다.
            bool isArriveDest = mapBoard.GetDistance2D(minCostNode.Terrain.gameObject, destinationObj).
                Equals(Vector2Int.zero);
            if(isArriveDest)
            {
                //{목적지에 도착했다면 aStarResultPath 리스트를 설정한다.
                AstarNode resultNode = minCostNode;
                bool isSet_aStarResultPathOk = false;
                while(isSet_aStarResultPathOk == false) 
                {
                    aStarResultPath.Add(resultNode);
                    if (resultNode.AstarPrevNode == default || resultNode.AstarPrevNode == null)
                    {
                        isSet_aStarResultPathOk = true;
                        break;
                    }
                    else {/*todo nothing*/ }

                    resultNode = resultNode.AstarPrevNode;
                } //loop: 이전노드를 찾지못할때까지 순회하는 루프
                //{목적지에 도착했다면 aStarResultPath 리스트를 설정한다.
                //openList와 closelist를 정리한다.
                aStarOpenPath.Clear();
                aStarClosePath.Clear();
                isFoundDestination = true;
                break;

            } // if: 선택한 노드가 목적지에 도착한경우
            else
            {
                //도착하지 않았다면 현재 타일을 기준으로 4방향 노드를 찾아온다.
                List<int> nextSearchIdx1Ds = mapBoard.GetTileIdx2D_Around4ways(minCostNode.Terrain.TileIdx2D);
                //찾아온 노드중에서 이동가능한 노드는 Open List에 추가한다.
                AstarNode nextNode = default;
                foreach(var nextIdx1D in nextSearchIdx1Ds)
                {
                    nextNode = new AstarNode(mapBoard.GetTerrain(nextIdx1D),destinationObj);
                    if(nextNode.Terrain.IsPassable == false) { continue; }

                    Add_AstarOpenList(nextNode, minCostNode);
                } //loop: 이동가능한 노드를 open list에 추가하는 루프
                //도착하지 않았다면 현재 타일을 기준으로 4방향 노드를 찾아온다.

                //탐색이 끝난 노드는 close list에 추가하고 openlist에서 제거한다.
                //이때 openlist가 비어있다면 더이상 탐색할수있는 길이 존재하지 않는것이다.

                aStarClosePath.Add(minCostNode);
                aStarOpenPath.Remove(minCostNode);
                if(aStarOpenPath.IsValid() == false)
                {
                    GFunc.LogWarning("[Warning] There are no more tiles to explore.");
                    isNowayToGo = true;
                } //if: 목적지에 도착하지 못했는데, 더이상 탐색할수있는 길이 없는 경우

                foreach(var tempNode in aStarOpenPath)
                {
                    GFunc.Log($"idx: {tempNode.Terrain.TileIdx1D}" + $"Cost: {tempNode.AstarF}");
                }
            }   //else : 선택한 노드가 목적지에 도착하지 못한경우

            loopIdx++;
            yield return new WaitForSeconds(delay_);
        } //Loop: Astar 알고리즘으로 길을 찾는 메인루프

        
    } //DelayFindPath_Astar()


    //! 비용을 설정한 노드를 오픈리스트에 추가하는 함수
    private void Add_AstarOpenList(AstarNode targetTerrain_, AstarNode prevNode =default)
    {
        //Open 리스트에 추가하기 전에 알고리즘 비용을 설정한다.
        Update_AstarCostToTerrain (targetTerrain_, prevNode);

        AstarNode closeNode = aStarClosePath.FindNode(targetTerrain_);
        if (closeNode != default && closeNode != null)
        {
            //이미 탐색이 끝난 좌표의 노드가 존재하는 경우에는 
            //openList에 추가하지 않는다.
        } //if : Close list에 이미 탐색이 끝난 좌표의 노드가 존재하는경우
        else
        {
            AstarNode openedNode = aStarOpenPath.FindNode(targetTerrain_);
            if (openedNode != default && openedNode != null)
            {
                //타겟 노드의 코스트가 더작은 경우에느 openList에서 노드를 교체한다.
                // 타겟 노드의 코스트가 더큰 경우에느 openList에 추가하지 않는다.
                if (openedNode != default && openedNode != null)
                {
                    aStarOpenPath.Remove(openedNode);
                    aStarOpenPath.Add(targetTerrain_);
                }
                else { /*do nothing*/}

            }   // if Open List 에 현재 추가할 노드와 같은 좌표의 노드가 존재하는경우
            else
            {
                aStarOpenPath.Add (targetTerrain_);
            }   // else : Open List에 현재 추가할 노드와 같은 좌표의 노드가 없는경우
        }   //else: 아직 탐색이 끝나지 않은 노드인 경우 

    }   //Add_AstarOpenList()




    //!Target 지형정보와 Destination 지형 정보로 Distance와 Heuristic을 설정하는함수
    private void Update_AstarCostToTerrain(
        AstarNode targetNode, AstarNode prevNode )
    {
        //Target 지형에서 Destination 까지의 2D 타일 거리를 계산하는 로직
        Vector2Int distance2D = mapBoard.GetDistance2D(targetNode.Terrain.gameObject, destinationObj);
        int totalDistance2D = distance2D.x + distance2D.y;

        //Heuristic 은 직선거리로 고정한다.
        Vector2  localDistance = destinationObj.transform.localPosition - targetNode.Terrain.transform.localPosition;
        float heuristic = Mathf.Abs(localDistance.magnitude);

        //이전노드가 존재하는 경우 이전노드의 코스트를 추가해서 연산한다.
        if(prevNode == default || prevNode == null) { /*do nothing*/}
        else
        {
            totalDistance2D = Mathf.RoundToInt(prevNode.AstarG + 1.0f);
        }   //G가 충분하지 않아서 보정했다.
        targetNode.UpdateCost_Astar(totalDistance2D, heuristic, prevNode);

    } // Update_AstarCostToTerrain()
}   // class PathFinder
