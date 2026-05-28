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
            public int NoRecordRevealed;
            public int AutoNoRecordP1;
            public int AutoNoRecordP2;
            public int AutoNoRecordTotal;
            public float AutoFillDurationSeconds;
            public TimeSpan SetupP1;
            public TimeSpan SetupP2;
            public int Rotations;
            public int InvalidPlacements;
            public int RepositionsOrRemovals;
            public int GuessAttemptsFromFreshDiscovery;
            public int GuessAttemptsFromPersistentAction;
            public int RepeatedGuessesAfterWrong;
            public int TurnsWithoutHiddenCards;
            public int EndgameSafeguardTriggered;
        }

        private Report report = new Report();
        private readonly Dictionary<string, int> cluesBeforeCorrectByCharacter = new Dictionary<string, int>();
        private DateTime setupStartP1;
        private DateTime setupStartP2;

        public void StartMatch(string modeName)
        {
            report = new Report();
            cluesBeforeCorrectByCharacter.Clear();
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
        public void OnNoRecordRevealed() => report.NoRecordRevealed++;
        public void OnAutoNoRecordGenerated(PlayerId player, int amount)
        {
            report.AutoNoRecordTotal += amount;
            if (player == PlayerId.PlayerOne) report.AutoNoRecordP1 += amount;
            else report.AutoNoRecordP2 += amount;
        }
        public void OnAutoFillDuration(float seconds) => report.AutoFillDurationSeconds += Mathf.Max(0f, seconds);
        public void OnRotate() => report.Rotations++;
        public void OnInvalidPlacement() => report.InvalidPlacements++;
        public void OnRepositionOrRemove() => report.RepositionsOrRemovals++;
        public void OnGuessAttemptSource(bool fromFreshDiscovery) { if (fromFreshDiscovery) report.GuessAttemptsFromFreshDiscovery++; else report.GuessAttemptsFromPersistentAction++; }
        public void OnRepeatedGuessAfterWrongAttempt(string characterId) { if (report.WrongGuesses > 0) report.RepeatedGuessesAfterWrong++; }
        public void OnNoHiddenCardsTurn() => report.TurnsWithoutHiddenCards++;
        public void OnEndgameSafeguardTriggered() => report.EndgameSafeguardTriggered++;

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
            if (report.NoRecordRevealed >= 3 && !report.FirstCharacterFound.HasValue) interpret += "Leitura: muitos Sem Registro apareceram antes do primeiro dossiê. Talvez o grid esteja grande demais ou a densidade de cartas úteis esteja baixa.\n";
            if (report.NoRecordRevealed > 0) interpret += "Leitura: Sem Registro funcionou como feedback de investigação sem recompensar diretamente o erro.\n";
            if (report.EndgameSafeguardTriggered > 0) interpret += "Leitura: o jogo entrou em modo de resolução porque não havia mais cartas ocultas úteis. Isso evitou soft lock.\n";
            if (report.WrongGuesses >= 3) interpret += "Leitura: os jogadores estão tentando identificar com poucas pistas. Talvez o Guia de Apoio precise ser mais útil ou destacado.\n";

            return $"[Resultado]\nModo jogado: {report.ModeName}\nVencedor: {report.Winner}\nPlacar final: J1 {report.ScoreP1} x {report.ScoreP2} J2\nDuração total da partida: {report.TotalDuration:mm\\:ss}\nTotal de turnos: {report.TotalTurns}\n\n" +
                   $"[Ritmo da partida]\nPrimeira investigação: {Fmt(report.FirstInvestigation)}\nPrimeiro personagem encontrado: {Fmt(report.FirstCharacterFound)}\nPrimeira pista solicitada: {Fmt(report.FirstClue)}\nPrimeira tentativa de identificação: {Fmt(report.FirstGuess)}\nPrimeira identificação correta: {Fmt(report.FirstCorrect)}\n\n" +
                   $"[Dedução]\nPersonagens encontrados: {report.CharactersFound}\nPersonagens identificados corretamente: {report.CharactersCorrect}\nTentativas erradas: {report.WrongGuesses}\nTentativas de identificação (descoberta fresca): {report.GuessAttemptsFromFreshDiscovery}\nTentativas de identificação (ação persistente): {report.GuessAttemptsFromPersistentAction}\nRepetições após erro: {report.RepeatedGuessesAfterWrong}\nMédia de pistas antes do acerto: {report.AvgCluesBeforeCorrect:0.00}\nCategoria de pista mais usada: {topClue}\n\n" +
                   $"[Pesquisa]\nUsos do Guia de Apoio: {report.GuidebookOpens}\nFichas de Pesquisa usadas: {report.ResearchUsed}\nO guia foi usado antes de algum acerto? {(report.GuidebookBeforeCorrect ? "Sim" : "Não")}\n\n" +
                   $"[Cartas de Arquivo]\nCartas de Arquivo reveladas: {report.ArchiveRevealed}\nTipo mais revelado: {MostArchiveType()}\nEfeitos sem alvo válido: {report.ArchiveEffectsNoTarget}\n\n" +
                   $"[Sem Registro]\nSem Registro revelados: {report.NoRecordRevealed}\n% de investigações em Sem Registro: {PctNoRecord():0.0}%\nTurnos sem cartas ocultas restantes: {report.TurnsWithoutHiddenCards}\nSafeguard anti-soft-lock acionado: {report.EndgameSafeguardTriggered}\n\n" +
                   $"[Montagem]\nTempo de montagem do Jogador 1: {report.SetupP1:mm\\:ss}\nTempo de montagem do Jogador 2: {report.SetupP2:mm\\:ss}\nSem Registro automáticos J1: {report.AutoNoRecordP1}\nSem Registro automáticos J2: {report.AutoNoRecordP2}\nSem Registro automáticos total: {report.AutoNoRecordTotal}\nDuração total do preenchimento automático: {report.AutoFillDurationSeconds:0.00}s\nRotações de carta: {report.Rotations}\nPosicionamentos inválidos: {report.InvalidPlacements}\n\n" +
                   $"{interpret}";
        }
        private float PctNoRecord() => report.TotalTurns <= 0 ? 0f : (report.NoRecordRevealed / Mathf.Max(1f, report.TotalTurns)) * 100f;

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
