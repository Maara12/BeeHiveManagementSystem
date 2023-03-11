using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BeeHiveManagementSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Queen queen = new Queen();
        DispatcherTimer timer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
            statusReport.Text = queen.StatusReport;
            timer.Interval = TimeSpan.FromSeconds(1.5);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            WorkNextShift_Click(this, new RoutedEventArgs());
        }

        private void JobSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void AssignBeeButton_Click(object sender, RoutedEventArgs e)
        {
            queen.AssignBee(JobSelector.Text);
            statusReport.Text = queen.StatusReport;
        }

        private void WorkNextShift_Click(object sender, RoutedEventArgs e)
        {
            queen.WorkTheNextShift();
            statusReport.Text = queen.StatusReport;

        }
    }

    static class HoneyVault
    {
        public static string statusReport
        {

            get
            {
                string defaultString = $"Vault Report :\n{honey} units of Honey" +
                    $"\n{nectar} units of Nectar";
                if (honey < LOW_LEVEL_WARNING) return defaultString + $"\nLOW HONEY - ADD A HONEY MANUFACTURER";
                else if (nectar < LOW_LEVEL_WARNING) return defaultString + $"\nLOW NECTAR- ADD A NECTAR COLLECTOR";
                else if (nectar < LOW_LEVEL_WARNING && honey < LOW_LEVEL_WARNING)
                {
                    return defaultString + $"\nLOW HONEY - ADD A HONEY MANUFACTURER"
                        + $"\nLOW NECTAR- ADD A NECTAR COLLECTOR";
                }
                else return defaultString;
            }
        }

        private const float NECTAR_CONVERSION_RATIO = .19f;
        private const float LOW_LEVEL_WARNING = 10f;

        private static float honey = 25f;
        private static float nectar = 100f;

        public static void CollectNectar(float amount)
        {
            if (amount > 0) nectar += nectar;
        }

        public static void ConvertNectarToHoney(float amount)
        {
            float nectarToConvert = amount;
            if (nectarToConvert > nectar) nectarToConvert = nectar;
            nectar -= nectarToConvert;
            honey += nectarToConvert * NECTAR_CONVERSION_RATIO;
        }

        public static bool ConsumeHoney(float amount)
        {
            if (amount <= honey)
            {
                honey -= amount;
                return true;
            }
            else return false;
        }

    }

    abstract class Bee
    {
        public Bee(string beeType)
        {
            Job = beeType;
        }

        public string Job { get; private set; }

        public abstract float CostPerShift { get; }

        public void WorkTheNextShift()
        {
            bool hasEnoughHoneyToWork = HoneyVault.ConsumeHoney(CostPerShift);
            if (hasEnoughHoneyToWork) DoJob();

        }

        protected abstract void DoJob();
       
    }

    class Queen : Bee
    {

        private Bee[] workers = new Bee[0];
        private float eggs;
        private float unassignedWorkers = 3;
        private const float EGGS_PER_SHIFT = 0.45f;
        private const float HONEY_PER_UNASSIGNED_WORKER = 0.5f;

        public Queen() : base("Queen")
        {
            AssignBee("NectarCollector");
            AssignBee("HoneyManufacturer");
            AssignBee("EggCare");

        }

        public string StatusReport { get; private set; }

        public override float CostPerShift
        {
            get { return 2.15f; }
        }

        /// <summary>
        /// Expand the workers array by one slot and add a Bee reference.
        /// </summary>
        /// <param name="worker">Worker to add to the workers array.</param>
        private void AddWorker(Bee worker)
        {
            if (unassignedWorkers >= 1)
            {
                unassignedWorkers--;
                Array.Resize(ref workers, workers.Length + 1);
                workers[workers.Length - 1] = worker;
            }
        }


        protected override void DoJob()
        {
            eggs += EGGS_PER_SHIFT;
            foreach (Bee worker in workers)
            {
                worker.WorkTheNextShift();
            }
            HoneyVault.ConsumeHoney(HONEY_PER_UNASSIGNED_WORKER * unassignedWorkers);
            UpdateStatusReport();
        }

        public void AssignBee(string Job)
        {
            switch (Job)
            {
                case "HoneyManufacturer": AddWorker(new HoneyManufacturer());
                    break;
                case "NectarCollector": AddWorker(new NectarCollector());
                    break;
                case "EggCare": AddWorker(new EggCare(this));
                    break;
            }
            UpdateStatusReport();
        }

        public void CareForEggs(float eggsToConvert)
        {
            if (eggs >= eggsToConvert)
            {
                eggs -= eggsToConvert;
                unassignedWorkers += eggsToConvert;
            }
        }

        private void UpdateStatusReport()
        {
            StatusReport = $"{HoneyVault.statusReport} \n\n " +
                 $"Egg Count : {eggs} \nUnassigned workers : {unassignedWorkers}" +
                 $"{workerStatus()} \nTOTAL WORKERS : {workers.Length}";
        }

        private string workerStatus()
        {
            int numOfNectarCollectors = 0;
            int numOfHoneyManufacturers = 0;
            int numOfEggCarers = 0;
            string result = "";

            for(int i =0; i < workers.Length; i++)
            {
                if (workers[i].Job == "HoneyManufacturer") numOfHoneyManufacturers++;
                else if (workers[i].Job == "NectarCollector") numOfNectarCollectors++;
                else if (workers[i].Job == "EggCare") numOfEggCarers++;
            }

            result = $"\n{numOfNectarCollectors} Nectar Collector " +
                $"\n{numOfHoneyManufacturers} Honey Manufacturer " +
                $"\n{numOfEggCarers} Egg Care ";

            return result;
        }
    }

    class HoneyManufacturer : Bee
    {

        private const float NECTAR_PROCESSED_PER_SHIFT = 33.15f;

        public HoneyManufacturer() : base("HoneyManufacturer")
        {

        }

        public override float CostPerShift
        {
            get { return 1.7f; }
        }

        protected override void DoJob()
        {
            HoneyVault.ConvertNectarToHoney(NECTAR_PROCESSED_PER_SHIFT);
        }
    }

    class NectarCollector : Bee
    {

        private const float NECTAR_COLLECTED_PER_SHIFT = 33.25f;

        public NectarCollector() : base("NectarCollector")
        {

        }

        public override float CostPerShift
        {
            get { return 1.95f; }
        }

        protected override void DoJob()
        {
            HoneyVault.CollectNectar(NECTAR_COLLECTED_PER_SHIFT);
        }
    }

    class EggCare : Bee
    {

        private const float CARE_PROGRESS_PER_SHIFT = 0.15f;
        private Queen queen;

        public EggCare(Queen queen) : base("EggCare")
        {
            this.queen = queen;
        }

        public override float CostPerShift
        {
            get { return 1.35f; }
        }

        protected override void DoJob()
        {
            queen.CareForEggs(CARE_PROGRESS_PER_SHIFT);
        }
    }
}
