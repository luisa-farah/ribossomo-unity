# Ribossomo

Projeto individual — Jogos Digitais (Insper), por **Luísa Carvalho Farah**.
Jogo 2D em Unity 6.3, construído a partir do tutorial "Introdução Rápida".

**Jogar no navegador:** <link do itch.io>

## Sobre o jogo
Você é um **ribossomo** no citoplasma. O HUD mostra a fita de **mRNA** (5' → 3') e a cadeia de
aminoácidos que ela codifica. Colete os aminoácidos **na ordem certa** para sintetizar a proteína.
Toda proteína começa com **Met (AUG)**, o códon de início.

| Aminoácido | Códon |
|---|---|
| Met | AUG |
| Gly | GGC |
| Phe | UUU |
| Leu | CUG |
| Ala | GCC |

- **Tempo como mecânica:** o cronômetro é regressivo. Cada proteína completa dá +2 s por aminoácido; aminoácido errado tira 3 s.
- **Vírus:** patrulham rotas fixas (um fica parado) e tiram 1 vida. Ficam mais rápidos a cada proteína.
- **Dificuldade:** as proteínas ficam mais longas (3 → 6 aminoácidos).
- **Fim:** quando o tempo acaba ou as 3 vidas acabam; a tela final mostra proteínas sintetizadas e tempo de sobrevivência.

## Controles
| Ação | Teclado | Controle (Xbox) |
|---|---|---|
| Mover | WASD / setas | Analógico esquerdo / D-pad |
| Reiniciar | R | Select (View) |
| Menu | Esc | Start (Menu) |
| Botões da UI | Mouse / Enter | A |

## Estrutura
- `Assets/Scripts/GameManager.cs` — estado estático da partida (proteínas, vidas, tempo) e troca de cenas
- `Assets/Scripts/ProteinSynthesis.cs` — regras da tradução, cronômetro, HUD, reposição de aminoácidos e fim de jogo
- `Assets/Scripts/PlayerMovement.cs` — movimento (Rigidbody2D), diagonal normalizada, coleta por trigger
- `Assets/Scripts/PlayerHealth.cs` — dano e invencibilidade temporária
- `Assets/Scripts/Virus.cs` — patrulha entre dois pontos
- `Assets/Scripts/AminoAcid.cs` — coletável (prefab) e tabela de códons
- `Assets/Scripts/MenuController.cs` — menu inicial e tela de fim
- `Assets/Editor/TutorialBuilder.cs` — monta cenas, prefab, sprites, sons e música (menu **Tutorial** no editor)
- Cenas: `MainMenu` → `Game` → `EndGame`

## Créditos
- Sprites (ribossomo, aminoácidos, vírus, organelas), efeitos sonoros e música: **gerados por código** no próprio projeto (`TutorialBuilder.cs`) — sem assets de terceiros.
- Fonte: LegacyRuntime (built-in do Unity).
- Desenvolvido com auxílio do Claude (Anthropic) para geração de código.
