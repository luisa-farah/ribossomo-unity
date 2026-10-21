# Ribossomo

Projeto individual — Jogos Digitais (Insper), por **Luísa Carvalho Farah**.
Jogo 2D em Unity 6.3, construído a partir do tutorial "Introdução Rápida".

**Jogar no navegador:** <link do itch.io>

## Sobre o jogo
Você é um **ribossomo** no citoplasma. O HUD mostra a fita de **mRNA** (5' → 3') e a cadeia que ela codifica.
Colete as peças **na ordem certa**: toda proteína começa com **Met (AUG)** e termina com o códon de parada **STOP (UAA)**, que a libera.

| Peça | Códon |
|---|---|
| Met (início) | AUG |
| Gly | GGC |
| Phe | UUU |
| Leu | CUG |
| Ala | GCC |
| STOP (parada) | UAA |

- **Tempo como mecânica:** cronômetro regressivo. Proteína completa = +2 s por peça; peça errada = −3 s.
- **Modo avançado:** a partir da 3ª proteína o HUD mostra só os códons — é preciso lembrar a tabela.
- **Vírus:** patrulham rotas fixas (um fica parado), tiram 1 vida e aceleram a cada proteína.
- **Power-ups:** ATP (velocidade por 4 s) e anticorpo (escudo contra 1 vírus).
- **Feedback:** partículas, textos flutuantes, tremida de câmera e flash de dano; organelas e membrana animadas.
- **Fim:** tempo zerado ou 3 vidas perdidas. A tela final mostra proteínas, tempo de sobrevivência, última proteína e recorde (salvo no navegador).
- Botão **Como jogar** no menu com regras e exemplos.

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
- `Assets/Scripts/PowerUp.cs` — power-ups ATP e anticorpo
- `Assets/Scripts/FX.cs`, `FXParticle.cs`, `FloatingText.cs`, `CameraShake.cs` — feedback visual
- `Assets/Scripts/Floater.cs`, `MembranePulse.cs` — fundo animado
- `Assets/Scripts/MenuController.cs` — menu inicial e tela de fim
- `Assets/Editor/TutorialBuilder.cs` — monta cenas, prefab, sprites, sons e música (menu **Tutorial** no editor)
- Cenas: `MainMenu` → `Game` → `EndGame`

## Créditos
- Sprites (ribossomo, aminoácidos, vírus, organelas), efeitos sonoros e música: **gerados por código** no próprio projeto (`TutorialBuilder.cs`) — sem assets de terceiros.
- Fonte: LegacyRuntime (built-in do Unity).
- Desenvolvido com auxílio do Claude (Anthropic) para geração de código.
