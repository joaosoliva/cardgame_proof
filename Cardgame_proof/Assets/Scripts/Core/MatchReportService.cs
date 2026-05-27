using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace CardgameProof.Core
{
    public sealed class MatchReportService
    {
        public sealed class Report
        {
            public string ModeName;
            public DateTime StartTimeUtc;
            public DateTime EndTimeUtc;
            public TimeSpan TotalDuration;
            public string Winner;
            public int ScoreP1;
            public int ScoreP2;
            public int TotalTurns;
            public int TurnsP1;
            public int TurnsP2;
            public TimeSpan? FirstInvestigation;
            public TimeSpan? FirstCharacterFound;
            public TimeSpan? FirstClue;
            public TimeSpan? FirstGuidebookUse;
            public TimeSpan? FirstGuess;
            public TimeSpan? FirstCorrect;
            public int CharactersFound;
            public int CharactersCorrect;
            public int WrongGuesses;
            public float AvgCluesBeforeCorrect;
            public Dictionary<ClueCategory, int> CluesByCategory = new Dictionary<ClueCategory, int>();
            public int GuidebookOpens;
            public int ResearchUsed;
            public int ResearchUsedP1;
            public int ResearchUsedP2;
            public bool GuidebookBeforeCorrect;
            public int ArchiveRevealed;
            public int ArchiveLacuna;
            public int ArchiveReferencia;
            public int ArchiveFragmento;
            public int ArchiveEffectsSuccess;
            public int ArchiveEffectsNoTarget;
            public TimeSpan SetupP1;
            public TimeSpan SetupP2;
            public int Rotations;
            public int InvalidPlacements;
            public int RepositionsOrRemovals;
        }

        private readonly Report report = new Report();
        private readonly Dictionary<string, int> cluesBeforeCorrectByCharacter = new Dictionary<string, int>();
        private DateTime setupStartP1;
        private DateTime setupStartP2;

        public void StartMatch(string modeName)
        {
            report.ModeName = modeName;
            report.StartTimeUtc = DateTime.UtcNow;
            report.CluesByCategory = Enum.GetValues(typeof(ClueCategory)).Cast<ClueCategory>().ToDictionary(x => x, _ => 0);
        }
        public void StartSetup(PlayerId player) { if (player == PlayerId.PlayerOne) setupStartP1 = DateTime.UtcNow; else setupStartP2 = DateTime.UtcNow; }
        public void EndSetup(PlayerId player) { if (player == PlayerId.PlayerOne) report.SetupP1 = DateTime.UtcNow - setupStartP1; else report.SetupP2 = DateTime.UtcNow - setupStartP2; }
        public void TurnStart(PlayerId player) { report.TotalTurns++; if (player == PlayerId.PlayerOne) report.TurnsP1++; else report.TurnsP2++; }
        public void MarkFirstInvestigation() { if (!report.FirstInvestigation.HasValue) report.FirstInvestigation = DateTime.UtcNow - report.StartTimeUtc; }
        public void MarkFirstCharacterFound() { if (!report.FirstCharacterFound.HasValue) report.FirstCharacterFound = DateTime.UtcNow - report.StartTimeUtc; report.CharactersFound++; }
        public void OnClueRequested(string characterId, ClueCategory category) { if (!report.FirstClue.HasValue) report.FirstClue = DateTime.UtcNow - report.StartTimeUtc; report.CluesByCategory[category]++; if (!cluesBeforeCorrectByCharacter.ContainsKey(characterId)) cluesBeforeCorrectByCharacter[characterId] = 0; cluesBeforeCorrectByCharacter[characterId]++; }
        public void OnGuidebookUse(PlayerId player) { if (!report.FirstGuidebookUse.HasValue) report.FirstGuidebookUse = DateTime.UtcNow - report.StartTimeUtc; report.GuidebookOpens++; report.ResearchUsed++; if (player == PlayerId.PlayerOne) report.ResearchUsedP1++; else report.ResearchUsedP2++; if (!report.FirstCorrect.HasValue) report.GuidebookBeforeCorrect = true; }
        public void OnGuess(bool correct, string characterId) { if (!report.FirstGuess.HasValue) report.FirstGuess = DateTime.UtcNow - report.StartTimeUtc; if (correct) { if (!report.FirstCorrect.HasValue) report.FirstCorrect = DateTime.UtcNow - report.StartTimeUtc; report.CharactersCorrect++; } else report.WrongGuesses++; }
        public void OnArchiveRevealed(string effectName) { report.ArchiveRevealed++; if (effectName.Contains("lacuna")) report.ArchiveLacuna++; else if (effectName.Contains("referencia")) report.ArchiveReferencia++; else report.ArchiveFragmento++; }
        public void OnArchiveResolution(bool success) { if (success) report.ArchiveEffectsSuccess++; else report.ArchiveEffectsNoTarget++; }
        public void OnRotate() => report.Rotations++;
        public void OnInvalidPlacement() => report.InvalidPlacements++;
        public void OnRepositionOrRemove() => report.RepositionsOrRemovals++;

        public Report Finish(string winner, int p1, int p2)
        {
            report.EndTimeUtc = DateTime.UtcNow;
            report.TotalDuration = report.EndTimeUtc - report.StartTimeUtc;
            report.Winner = winner;
            report.ScoreP1 = p1;
            report.ScoreP2 = p2;
            if (report.CharactersCorrect > 0)
                report.AvgCluesBeforeCorrect = cluesBeforeCorrectByCharacter.Values.Sum() / (float)report.CharactersCorrect;
            SaveJson();
            return report;
        }

        public string BuildReadableReport()
        {
            string topClue = report.CluesByCategory.OrderByDescending(x => x.Value).First().Key switch { ClueCategory.Area => "Área", ClueCategory.Era => "Época", ClueCategory.Region => "Região", ClueCategory.Contribution => "Contribuição", _ => "Contexto/Legado" };
            string interpret = "";
            if (report.FirstCorrect.HasValue && report.FirstCorrect.Value.TotalMinutes > 5) interpret += "Leitura: a partida demorou para gerar a primeira identificação. Talvez as pistas estejam difíceis ou existam cartas demais no tabuleiro.\n";
            if (report.WrongGuesses >= 3) interpret += "Leitura: os jogadores tentaram adivinhar cedo demais. Talvez seja necessário reforçar o tutorial sobre coletar pistas.\n";
            if (report.GuidebookOpens == 0) interpret += "Leitura: o Guia de Apoio não foi usado. Talvez o botão precise ficar mais visível ou a regra de pesquisa precise ser melhor explicada.\n";
            if (report.InvalidPlacements >= 3) interpret += "Leitura: a montagem do tabuleiro gerou confusão. Talvez seja necessário melhorar o feedback visual de posicionamento.\n";
            if (report.ArchiveEffectsNoTarget >= 2) interpret += "Leitura: algumas Cartas de Arquivo não conseguiram resolver seus efeitos. Talvez os efeitos precisem ser mais simples ou flexíveis.\n";
            if (report.TotalDuration.TotalMinutes <= 5 && report.CharactersCorrect > 0) interpret += "Leitura: o ritmo da partida parece adequado para demonstração curta.\n";

            return $"[Resultado]\nModo jogado: {report.ModeName}\nVencedor: {report.Winner}\nPlacar final: J1 {report.ScoreP1} x {report.ScoreP2} J2\nDuração total da partida: {report.TotalDuration:mm\\:ss}\nTotal de turnos: {report.TotalTurns}\n\n" +
                   $"[Ritmo da partida]\nPrimeira investigação: {Fmt(report.FirstInvestigation)}\nPrimeiro personagem encontrado: {Fmt(report.FirstCharacterFound)}\nPrimeira pista solicitada: {Fmt(report.FirstClue)}\nPrimeira tentativa de identificação: {Fmt(report.FirstGuess)}\nPrimeira identificação correta: {Fmt(report.FirstCorrect)}\n\n" +
                   $"[Dedução]\nPersonagens encontrados: {report.CharactersFound}\nPersonagens identificados corretamente: {report.CharactersCorrect}\nTentativas erradas: {report.WrongGuesses}\nMédia de pistas antes do acerto: {report.AvgCluesBeforeCorrect:0.00}\nCategoria de pista mais usada: {topClue}\n\n" +
                   $"[Pesquisa]\nUsos do Guia de Apoio: {report.GuidebookOpens}\nFichas de Pesquisa usadas: {report.ResearchUsed}\nO guia foi usado antes de algum acerto? {(report.GuidebookBeforeCorrect ? "Sim" : "Não")}\n\n" +
                   $"[Cartas de Arquivo]\nCartas de Arquivo reveladas: {report.ArchiveRevealed}\nTipo mais revelado: {MostArchiveType()}\nEfeitos sem alvo válido: {report.ArchiveEffectsNoTarget}\n\n" +
                   $"[Montagem]\nTempo de montagem do Jogador 1: {report.SetupP1:mm\\:ss}\nTempo de montagem do Jogador 2: {report.SetupP2:mm\\:ss}\nRotações de carta: {report.Rotations}\nPosicionamentos inválidos: {report.InvalidPlacements}\n\n" +
                   $"{interpret}";
        }

        private string MostArchiveType()
        {
            if (report.ArchiveLacuna >= report.ArchiveReferencia && report.ArchiveLacuna >= report.ArchiveFragmento) return "Lacuna de Arquivo";
            if (report.ArchiveReferencia >= report.ArchiveFragmento) return "Referência Cruzada";
            return "Fragmento de Documento";
        }
        private static string Fmt(TimeSpan? ts) => ts.HasValue ? ts.Value.ToString(@"mm\:ss") : "-";
        private void SaveJson() { File.WriteAllText(Path.Combine(Application.persistentDataPath, "match_report.json"), JsonUtility.ToJson(new Wrapper { text = BuildReadableReport() }, true)); }
        [Serializable] private sealed class Wrapper { public string text; }
    }
}
