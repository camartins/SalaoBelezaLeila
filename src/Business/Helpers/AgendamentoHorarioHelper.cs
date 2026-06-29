using Domain.Entities;

namespace Business.Helpers
{
    public static class AgendamentoHorarioHelper
    {
        public static int CalcularDuracaoTotalMinutos(IEnumerable<Servico> servicos)
            => servicos.Sum(s => s.DuracaoMinutos);

        public static int CalcularDuracaoTotalMinutos(IEnumerable<AgendamentoServico> itens)
            => itens.Sum(i => i.Servico?.DuracaoMinutos ?? 0);

        public static DateTime CalcularDataHoraFim(DateTime inicio, int duracaoMinutos)
            => inicio.AddMinutes(duracaoMinutos);

        /// <summary>
        /// Verifica sobreposição usando intervalo [inicio, fim) — o fim é exclusivo,
        /// permitindo que um agendamento comece exatamente quando outro termina.
        /// </summary>
        public static bool HorariosConflitam(DateTime inicio1, DateTime fim1, DateTime inicio2, DateTime fim2)
            => inicio1 < fim2 && inicio2 < fim1;

        public static string FormatarIntervalo(DateTime inicio, DateTime fim)
            => $"{inicio:dd/MM/yyyy HH:mm} às {fim:HH:mm}";
    }
}
