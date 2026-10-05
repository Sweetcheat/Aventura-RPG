Aventura-RPG
============

Um jogo simples de RPG. O código foi todo escrito e comentado por mim (Lucas Barbosa).
Este é um jogo de RPG com propósito estudantil, sem fins lucrativos.

Eu ainda vou adicionar alguns "features" no jogo que está faltando, como salvamento do progresso via arquivo XML.
O projeto está disponível em: https://github.com/Sweetcheat/Aventura-RPG

Arquitetura atual: Godot 4.7 (C#) + Motor (.NET 8).

- `AventuraRPG.Godot/` — apresentação (Godot 4.7 C#): input, UI, mundo 2D.
- `Motor/` — regras do jogo (Partida), .NET 8, independente de UI.
- `Motor.Tests/` — testes das regras do Motor (rodam com `dotnet run --project Motor.Tests`).

Para rodar: abra o projeto `AventuraRPG.Godot` no Godot 4.7 (export preset .NET) e execute.
