# NoMoreGoblin

마을을 지켜라 — 고블린 기지를 모두 파괴하는 탑다운 액션 생존 게임  
Unity 6 · Android · 개인 프로젝트 · 2025

---

## 게임 개요

플레이어는 마을 감독관이 되어 시민을 보호하고 경비원을 배치하며, 맵 곳곳에 흩어진 **7개의 고블린 기지**를 모두 파괴하면 승리한다. 고블린은 시민을 납치하고 건물을 파괴하며 시간이 지날수록 기지당 증원이 강화된다.

---

| 분류 | 내용 |
|------|------|
| 엔진 | Unity 6 · URP Universal 2D |
| 언어 | C# |
| UI | UGUI · TextMeshPro |

---

## 핵심 기능

### 1. FSM 기반 AI (고블린 · 시민 · 경비원)

고블린, 시민, 경비원 세 종류의 NPC 모두 **Finite State Machine** 구조로 행동을 관리한다. 각 엔티티는 `State` 추상 기반 클래스 → 구체 상태 → `FSM` 관리자 의 계층을 가진다.

**고블린 상태 전이:**

```
Idle ─→ Patrol ─→ Chase ─→ Attack
                 └──→ Kidnap (시민 납치)
```

- **Idle** — 2~5초 대기 후 순찰 또는 추적으로 전환, 1초마다 타겟 탐색
- **Patrol** — 기지 원점 40f 반경 내 SafePatrolPoint 순찰
- **Chase** — 1초마다 더 가까운 타겟으로 재조준, 탐지 범위 1.2배 이상이면 추적 포기
- **Attack** — 콜라이더 기준 거리 계산으로 정밀한 사거리 판정, 대상 유형(Player/Guard/Building)별 데미지 분기
- **Kidnap** — 시민을 기지까지 끌고가 100 데미지 처리 후 기지 용량 증가

관련 소스:
- [`GoblinFSM.cs`](https://github.com/Ellug/NoMoreGoblin/blob/main/Assets/Scripts/Enemy/FSM/GoblinFSM.cs)
- [`GoblinChaseState.cs`](https://github.com/Ellug/NoMoreGoblin/blob/main/Assets/Scripts/Enemy/FSM/GoblinChaseState.cs)
- [`GoblinKidnapState.cs`](https://github.com/Ellug/NoMoreGoblin/blob/main/Assets/Scripts/Enemy/FSM/GoblinKidnapState.cs)
- [`CitizenFSM.cs`](https://github.com/Ellug/NoMoreGoblin/blob/main/Assets/Scripts/NPC/Citizen/FSM/CitizenFSM.cs)
- [`GuardFSM.cs`](https://github.com/Ellug/NoMoreGoblin/blob/main/Assets/Scripts/NPC/Guard/FSM/GuardFSM.cs)

---

### 2. NPC 이동 — 장애물 회피 알고리즘

NPC가 건물·나무·다른 NPC 사이를 자연스럽게 통과하도록 **16방향 스캔 기반 회피 알고리즘**을 직접 구현했다. 유니티 내장 NavMesh는 동적 장애물에 취약하고 2D 환경 최적화가 어렵기 때문에, `Rigidbody2D.Slide()` 기반의 커스텀 컨트롤러를 설계했다.

**알고리즘 핵심 로직:**

| 상태 | 조건 | 동작 |
|------|------|------|
| 정상 이동 | 막힌 프레임 < 2 | 목표 방향으로 Slide |
| 회피 시작 | 막힌 프레임 ≥ 2 | 16방향 중 가장 열린 방향 선택, 0.35s 잠금 |
| 강제 탈출 | 막힌 프레임 ≥ 5 | 장애물 반대 방향으로 강제 이동 |

- 도착 판정: 거리 ≤ 0.18f (목표 버퍼 0.25f)
- 장애물 레이어: Wall, Tree, Enemy, NPC

관련 소스: [`NpcMoveController.cs`](https://github.com/Ellug/NoMoreGoblin/blob/main/Assets/Scripts/Utils/NpcMoveController.cs)

---

### 3. MVC 패턴 기반 엔티티 설계

플레이어·시민·경비원 모두 **Model / View / Controller** 3계층으로 책임을 분리했다. 모델은 C# 이벤트로 상태 변화를 알리고, 뷰는 이를 구독해 UI를 갱신한다.

```
PlayerController ─→ PlayerModel (OnHpChanged, OnExpChanged, OnLevelUp, OnDie)
                         └──→ PlayerView (HP/EXP 슬라이더 갱신)
```

관련 소스:
- [`PlayerController.cs`](https://github.com/Ellug/NoMoreGoblin/blob/main/Assets/Scripts/Player/PlayerController.cs)
- [`PlayerModel.cs`](https://github.com/Ellug/NoMoreGoblin/blob/main/Assets/Scripts/Player/PlayerModel.cs)
- [`PlayerView.cs`](https://github.com/Ellug/NoMoreGoblin/blob/main/Assets/Scripts/Player/PlayerView.cs)
- [`CitizenController.cs`](https://github.com/Ellug/NoMoreGoblin/blob/main/Assets/Scripts/NPC/Citizen/CitizenController.cs)
- [`GuardController.cs`](https://github.com/Ellug/NoMoreGoblin/blob/main/Assets/Scripts/NPC/Guard/GuardController.cs)

---

### 4. 오브젝트 풀링 (고블린 · 시민 · 경비원)

빈번한 생성·파괴로 인한 GC 부하를 방지하기 위해 세 종류의 NPC 모두 **큐 기반 오브젝트 풀**을 구현했다. 각 풀은 씬 시작 시 일정 수를 미리 인스턴스화하고 요청 시 반환한다.

| 풀 | 초기 용량 |
|----|---------|
| GoblinPool | 100 |
| CitizenPool | 50 |
| GuardPool | 50 |

관련 소스:
- [`GoblinPool.cs`](https://github.com/Ellug/NoMoreGoblin/blob/main/Assets/Scripts/Enemy/GoblinPool.cs)
- [`CitizenPool.cs`](https://github.com/Ellug/NoMoreGoblin/blob/main/Assets/Scripts/NPC/Citizen/CitizenPool.cs)
- [`GuardPool.cs`](https://github.com/Ellug/NoMoreGoblin/blob/main/Assets/Scripts/NPC/Guard/GuardPool.cs)

---

### 5. 자원 경제 시스템

| 자원 | 획득 | 소모 |
|------|------|------|
| 목재 (Wood) | 나무 벌채 (1~3) | 건물 건설 |
| 식량 (Food) | 30초마다 NPC 수 만큼 자동 생산 | 30초마다 경비원 수 × 2 소모, 경비원 고용 시 3 소모 |

경비원 배치 전략과 시민 수 관리가 핵심 자원 루프를 형성한다.

관련 소스: [`ResourceManager.cs`](https://github.com/Ellug/NoMoreGoblin/blob/main/Assets/Scripts/System/ResourceManager.cs)

---

### 6. 고블린 기지 스케일링

기지가 파괴될수록 남은 기지들이 강화되어 게임 후반으로 갈수록 압박이 증가한다.

- 맵 생성 시 7개 기지 랜덤 배치 (시작 지점 200f 이상, 기지 간 60f 이상 간격)
- 기지 파괴 시: 남은 기지들 용량 +10, 생성 수 증가
- 기지 소속 고블린은 생존한 임의의 기지로 재배정
- 모든 기지 파괴 → 게임 클리어

관련 소스:
- [`GoblinBaseManager.cs`](https://github.com/Ellug/NoMoreGoblin/blob/main/Assets/Scripts/System/GoblinBaseManager.cs)
- [`GoblinBase.cs`](https://github.com/Ellug/NoMoreGoblin/blob/main/Assets/Scripts/Enemy/GoblinBase.cs)

---

### 7. 건물 건설 시스템

타일맵 기반 격자 위에 울타리와 막사를 배치한다. 건설 중 마우스 위치에 프리뷰가 따라다니며, 배치 가능 여부에 따라 녹색/적색으로 피드백을 준다.

- **울타리 (Building)**: 고블린 진입 차단, 내구도 보유
- **막사 (GuardBarrack)**: 최대 4명의 경비원 자동 생성 (식량 3 소모)
- **집 (House)**: 최대 6명의 시민 자동 생성; 60초 이상 적이 없으면 "안전" 상태로 전환

관련 소스:
- [`BuildManager.cs`](https://github.com/Ellug/NoMoreGoblin/blob/main/Assets/Scripts/System/BuildManager.cs)
- [`House.cs`](https://github.com/Ellug/NoMoreGoblin/blob/main/Assets/Scripts/Build/House.cs)
- [`GuardBarrack.cs`](https://github.com/Ellug/NoMoreGoblin/blob/main/Assets/Scripts/Build/GuardBarrack.cs)

---

### 8. 플레이어 레벨업 시스템

나무 벌채 및 전투로 EXP를 획득하면 레벨업 시 3가지 강화 옵션 중 하나를 선택한다. 게임이 일시정지되고 옵션을 고르면 즉시 스탯이 적용된다.

| 강화 항목 | 최대 레벨 |
|-----------|---------|
| 이동 속도 | 10 |
| 공격력 | 10 |
| 공격 속도 | 10 |
| 공격 범위 | 12 |
| 최대 HP | 10 |

관련 소스:
- [`LevelupManager.cs`](https://github.com/Ellug/NoMoreGoblin/blob/main/Assets/Scripts/System/LevelupManager.cs)
- [`LevelupUI.cs`](https://github.com/Ellug/NoMoreGoblin/blob/main/Assets/Scripts/UI/LevelupUI.cs)

---

### 9. 스프라이트 정렬 (탑다운 깊이 표현)

탑다운 뷰에서 Y축 기반 깊이감을 표현하기 위해 스프라이트 하단 Y 좌표를 기준으로 Sorting Order를 동적으로 계산하는 시스템을 구현했다.

```
sortingOrder = -(int)((bottomY + worldOffsetY) * multiplier)
```

- `DynamicSortedObject` — 이동하는 캐릭터·NPC에 부착, SortManager에 자동 등록/해제
- `StaticSortedObject` — 나무·건물 등 정적 오브젝트에 부착, Awake 시 1회 계산

관련 소스:
- [`SortManager.cs`](https://github.com/Ellug/NoMoreGoblin/blob/main/Assets/Scripts/System/SortManager.cs)
- [`DynamicSortedObject.cs`](https://github.com/Ellug/NoMoreGoblin/blob/main/Assets/Scripts/Utils/DynamicSortedObject.cs)
- [`StaticSortedObject.cs`](https://github.com/Ellug/NoMoreGoblin/blob/main/Assets/Scripts/Utils/StaticSortedObject.cs)

---

## 아키텍처 개요

```
GameManager (씬 전환 · 일시정지 · 승패 판정)
  ├─ GoblinBaseManager      (기지 7개 배치 · 스케일링 · 클리어 판정)
  │    └─ GoblinBase × 7   (스폰 · 납치 관리)
  │         └─ GoblinPool   (100개 선할당)
  ├─ BuildingStructureManager (건물 레지스트리)
  │    ├─ House × N          → CitizenPool (50개)
  │    └─ GuardBarrack × N   → GuardPool  (50개)
  ├─ ResourceManager         (Wood · Food · NPC · Guard)
  ├─ LevelupManager          (강화 선택 UI)
  ├─ SortManager             (Y축 깊이 정렬)
  ├─ BuildManager            (타일맵 배치)
  ├─ TreeSpawnerManager      (나무 2000그루 절차적 배치)
  └─ FloatingTextManager     (데미지·자원 텍스트)
```

---

## 폴더 구조

```
Assets/Scripts/
├─ Build/           # BaseBuilding, House, GuardBarrack, BuildingData SO
├─ Enemy/
│   ├─ FSM/         # GoblinFSM, 5개 상태 클래스
│   └─ GoblinBase, GoblinPool, GoblinSpawner
├─ Interface/       # IDamageable
├─ NPC/
│   ├─ Citizen/     # CitizenController/Model/View, Pool, Spawner, FSM 4상태
│   └─ Guard/       # GuardController/Model/View, Pool, Spawner, FSM 4상태
├─ Player/
│   ├─ FSM/         # PlayerFSM, 4개 상태 클래스
│   └─ PlayerController/Model/View
├─ Resource/        # TreeObj
├─ System/          # GameManager, BuildManager, ResourceManager 등 7개
├─ UI/              # FloatingText, LevelupUI, MinimapController
└─ Utils/           # NpcMoveController, SafePatrolPoint, SortedObject
```
