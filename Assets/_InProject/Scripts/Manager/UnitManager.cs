using System.Collections.Generic;
using UnityEngine;

//  UnitManager는 유닛 생성 결과를 등록하고 찾아주는 역할만 맡기면 됩니다.
//   작업 순서:
//   1. Unit.cs 생성
//       - 공통 유닛 인스턴스: UnitId, UnitRuntimeRoot, 초기화/해제 책임
//   2. UnitManager에 유닛 컬렉션 추가
//       - Dictionary<int, Unit> 또는 ID 타입 기준 딕셔너리
//   3. 등록/해제 API 작성
//       - Register(Unit unit)
//       - Unregister(Unit unit)
//       - 중복 ID는 오류 로그 후 등록 거절
//   4. 조회 API 작성
//       - GetUnit(id)
//       - TryGetUnit(id, out unit)
//       - PlayerUnit 참조 또는 GetPlayerUnit() 제공
//   5. 생명주기 연결
//       - UnitLoader가 생성·초기화 후 UnitManager.Register() 호출
//       - 유닛 제거 시 Unregister() 후 GameObject 정리
//   여기에는 HP, 이동, Input, FSM, 전투 로직을 넣지 마세요. 그건 Unit의 하위
//   컴포넌트/각 시스템 책임입니다.

public class UnitManager : MonoBehaviour
{
    private UnitDefinition _unitDefinition;
    private List<PlayerUnit> _playerUnits = new List<PlayerUnit>();

    private void Awake()
    {

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
