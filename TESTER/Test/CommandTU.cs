
namespace TESTER
{
    public class CommandTU
    {
        /// <summary>
        /// Номер станции
        /// </summary>
        public int StationNumber { get; set; }
        /// <summary>
        /// Название команды ТУ
        /// </summary>
        public string NameTU { get; set; }
        /// <summary>
        /// Выполняется ли сейчас команда
        /// </summary>
        public bool isRun { get; set; }
    }
}
