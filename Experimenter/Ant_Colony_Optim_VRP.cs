using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;



namespace Experimenter
{
	internal class Ant_Colony_Optim_VRP
	{
		private Random random = new Random(0);

		public  double alpha{get; set;}
		
		public double beta{get; set;}

		public  double rho{ get; set; }
		public double Q{ get; set; }

		public  int numCities{ get; set; }
		
		public int numAnts{ get; set; } 
		public int  maxTime{ get; set; }

		public bool partial{get; set;}

		public int  maxRand;
		public string Report { get; set; }

		public int[] Trail { get; set; }
		public int[] Tour { get; set; }
		public int[] demand { get; set; } 





		public FileInfo Actualfile;
		



		
		

		

		public double BestInitial {get; set;}
		public double Best { get; set; }
		public int Time {get; set;}
	   



		public List<Microsoft.Maps.MapControl.WPF.Location> locations_ = new List<Microsoft.Maps.MapControl.WPF.Location>(); 
		public int vehicle { get; set; }
	   

		
		public Ant_Colony_Optim_VRP(string ProblemPath, double Alpha, double Beta, double Rho, double Q_ , int NumCities, int NumAnts, int MaxTime)
		{
			alpha = Alpha;
			beta = Beta;
			rho = Rho;
			Q = Q_ ;
			numCities = NumCities;
			numAnts = NumAnts;
			maxTime = MaxTime;
			partial = false ;
		   

		   
		try
			{
			   
				double[][]  Rawdists = ReadDir(ProblemPath);
				double[][] dists = update_Matrix(Rawdists);






				int[][]  ants = InitAnts(numAnts, numCities); // initialize ants to random trails

				int[]  bestTrail = BestTrail(ants, demand, Rawdists); // determine the best initial trail

			

				double bestLength = Length(bestTrail, demand, Rawdists); // the length of the best initial trail

				BestInitial = bestLength;
					 
				double[][]  pheromones = InitPheromones(numAnts, numCities);

				int time = 0;

				while (time < maxTime)
				{
					UpdateAnts(ants, pheromones, Rawdists);
					UpdatePheromones(pheromones, ants, demand,  Rawdists);

					int[]  currBestTrail = BestTrail(ants, demand, Rawdists);

					double currBestLength = Length(currBestTrail,demand, Rawdists);
					if (currBestLength < bestLength)
					{
						bestLength = currBestLength;
						bestTrail = currBestTrail;
						Time = time; 
					}

					++time;
				}
				
				Trail = bestTrail; Best = bestLength;

				Tour = Tours(bestTrail, demand);
				

			}
			catch (Exception ex)
			{
				if (MessageBox.Show(ex.Message,"Runtime error", MessageBoxButton.OK) == MessageBoxResult.OK)
				{
					Application.Current.Shutdown();
				}
			}
		}


		 double[][] update_Matrix( double[][] temp) //Helper for distance array without a supplier

		 {

			double[][] temparray = new double[(temp.Length)-1][];
			for (int i = 0; i < ((temp.Length) - 1); ++i)
				temparray[i] = new double[temp.Length - 1];

			for (int i = 1; i < temp.Length; i++)
			for (int j = 1; j < temp.Length; j++)
			temparray [i-1][j-1]= Convert.ToDouble(temp[i][j]);
			return temparray; 

		 }



		 double[][] ReadDir(string p)
		 {
			 string[] filePaths = Directory.GetFiles(p);

			 double[][] distances_ = new double[numCities][];
			 for (int i = 0; i < distances_.Length; ++i)
				 distances_[i] = new double[numCities];
			 



			 foreach (string s in filePaths)
			 {
				 if (s.EndsWith("vrp"))
				 {
					 string locationfile = File.ReadAllText(s);

					 ParseLoc loc = new ParseLoc(locationfile);

					 demand = loc.Demand.ToArray();

					 locations_ = loc.Locations;

				 }





				 


				 for (int i = 0; i < distances_.Length; i++)
				 {
					 for (int j = 0; j < distances_.Length; j++)
					 {
						 if (i == j)
						 {
							 distances_[i][j] = distances_[j][i] = 0;

						 }

						 else
						 {
							 double x0 = Convert.ToDouble(locations_[i].Longitude);
							 double x1 = Convert.ToDouble(locations_[i].Latitude);

							 double y0 = Convert.ToDouble(locations_[j].Longitude);
							 double y1 = Convert.ToDouble(locations_[j].Latitude);




							 double dx = x0 - y0;
							 double dy = x1 - y1;



							 distances_[i][j] = Convert.ToInt32(Math.Sqrt(dx * dx + dy * dy));
						 }
					 }



				 }

				 using (StreamWriter wr = new StreamWriter("Distance_Matrix.dat"))
				 {


					 for (int i = 0; i < numCities; i++)
					 {
						 String line = "";
						 for (int j = 0; j < numCities; j++)
						 {
							 line += distances_[i][j].ToString() + " ";
						 }

						 wr.WriteLine(line);
					 }


				 }


				 
			 }

			 return distances_;
		 
		 }




		 int[][] InitAnts(int numAnts, int numCities)
		{
			int[][] ants = new int[numAnts][];
			for (int k = 0; k < numAnts; ++k)
			{
				int start = random.Next(0, numCities);
				ants[k] = RandomTrail(start, numCities);
			}
			return ants;
		}

		 int[] RandomTrail(int start, int numCities) // helper for InitAnts
		{
			int[] trail = new int[numCities-1];

			for (int i = 0; i < numCities-1; ++i) { trail[i] = i; } // sequential

			for (int i = 0; i < numCities-1; ++i) // Fisher-Yates shuffle
			{
				int r = random.Next(i, numCities-1);
				int tmp = trail[r]; trail[r] = trail[i]; trail[i] = tmp;
			}

			//int idx = IndexOfTarget(trail, start); // put start at [0]
			//int temp = trail[0];
			//trail[0] = trail[idx];
			//trail[idx] = temp;

			return trail;
		}

		 int IndexOfTarget(int[] trail, int target) // helper for RandomTrail
		{
			for (int i = 0; i < trail.Length; ++i)
			{
				if (trail[i] == target)
					return i;
			}
			throw new Exception("Target not found in IndexOfTarget");
		}





		 int[] Tours(int[] trail, int[] demand) //Constructs tours based on returned trails (no demand is higher than the vehicle cap)
		 {

			 //this stence is used when a trail contains zero in order to work with non zero indexes only, nothing special (could be inmproved)
			 bool containszero = false;

			 for (int i = 0; i < trail.Length; i++)
			 {
				 if (trail[i] == 0)
				 {
					 containszero = true;

				 }
			 }


			 if (containszero)
			 {
				 for (int i = 0; i < trail.Length; i++)
				 {
					 trail[i] = trail[i] + 1; // this shifts up all the values by 1
				 }

				 // stence ends here 


			 }





			 // The demand array will always be +1 item than the trail or ants or numcities (+vehicle cap) 
			 int[] demand_ = new int[demand.Length];

			 for (int i = 0; i < demand.Length; i++)
			 {

				 demand_[i] = demand[i];
			 }

			 int VehicleLoad = 0;
			 //int Actualload = 0;
			 int VehicleCapacity = demand_[0];

			 List<int> tours = new List<int>();


			 //openning the tour starting from the supplier
			 tours.Add(0);


			 int j = 0;

			 do
			 {

				 VehicleLoad += demand_[trail[j]]; //Estimate load if client is accepted

				 if (VehicleLoad > VehicleCapacity)
				 {
					 //Refuse client 
					 //go back to depot 
					 //Vehicle load is reset and the vehicle can again deliver the client number j
					 //The client will be added in the next loop

					 if (partial)
					 {
						 //partial delivery is on 
						 //Client is accepted under rules 


						 //int delivery = VehicleCapacity - (VehicleLoad - demand_[trail[j]]);

						 //demand_[trail[j]] -= delivery;


						 if ((VehicleLoad - demand_[trail[j]]) < VehicleCapacity)
						 {
							 demand_[trail[j]] = VehicleLoad - VehicleCapacity;

							 tours.Add(trail[j]);


						 }

					 }


					 tours.Add(0);
					 VehicleLoad = 0;

				 }


				 else if (VehicleLoad <= VehicleCapacity)
				 {
					 //Accept client 
					 tours.Add(trail[j]);
					 j++;
					 continue;
				 }

			 } while (j < trail.Length);

			 // Closing the tour
			 tours.Add(0);
			 int[] toursarray = tours.ToArray();
			 return toursarray;

		 }

		 double Length(int[] trail, int[] demand, double[][] Rawdists) // total length of a trail
		 {

			 //int[] tour = Tours(trail, demand);
			 int[] tour = Tours(trail, demand);

			 double result = 0.0;




			 for (int i = 0; i < tour.Length - 1; ++i)
				 result += Distance(tour[i], tour[i + 1], Rawdists);
			 return result;
		 }


		// --------------------------------------------------------------------------------------------

		 int[] BestTrail(int[][] ants, int[] demand, double[][] Rawdists) // best trail has shortest total length
		 {
			 double bestLength = Length(ants[0], demand, Rawdists);
			 int idxBestLength = 0;
			 for (int k = 1; k < ants.Length; ++k)
			 {
				 double len = Length(ants[k], demand, Rawdists);
				 if (len < bestLength)
				 {
					 bestLength = len;
					 idxBestLength = k;
				 }
			 }
			 int numCities_ = ants[0].Length;
			 int[] bestTrail = new int[numCities_];
			 ants[idxBestLength].CopyTo(bestTrail, 0);
			 return bestTrail;
		 }

		// --------------------------------------------------------------------------------------------

		 double[][] InitPheromones(int numAnts, int numCities)
		 {
			 double[][] pheromones = new double[numAnts][];
			 for (int i = 0; i < numAnts; ++i)
				 pheromones[i] = new double[numCities-1];
			 for (int i = 0; i < pheromones.Length; ++i)
				 for (int j = 0; j < pheromones[i].Length; ++j)
					 pheromones[i][j] = 0.0001; // otherwise first call to UpdateAnts -> BuiuldTrail -> NextNode -> MoveProbs => all 0.0 => throws
			 return pheromones;
		 }

		// --------------------------------------------------------------------------------------------

		 void UpdateAnts(int[][] ants, double[][] pheromones, double[][] dists)
		 {
			 int numCities_ = pheromones[0].Length ;
			 for (int k = 0; k < ants.Length; ++k)
			 {
				 int start = random.Next(1, numCities_);
				 int[] newTrail = BuildTrail(k, start, pheromones, dists);
				 ants[k] = newTrail;
			 }
		 }


		 int[] BuildTrail(int k, int start, double[][] pheromones, double[][] dists)
		 {
			 int numCities_ = pheromones[0].Length;
			 int[] trail = new int[numCities_];
			 bool[] visited = new bool[numCities_];
			 trail[0] = start;
			 visited[start] = true;
			 for (int i = 0; i < numCities_ - 1; ++i)
			 {
				 int cityX = trail[i];
				 int next = NextCity(k, cityX, visited, pheromones, dists);
				 trail[i + 1] = next;
				 visited[next] = true;
			 }
			 return trail;
		 }

		 int NextCity(int k, int cityX, bool[] visited, double[][] pheromones, double[][] dists)
		 {
			 // for ant k (with visited[]), at nodeX, what is next node in trail?
			 double[] probs = MoveProbs(k, cityX, visited, pheromones, dists);

			 double[] cumul = new double[probs.Length + 1];
			 for (int i = 0; i < probs.Length; ++i)
				 cumul[i + 1] = cumul[i] + probs[i]; // consider setting cumul[cuml.Length-1] to 1.00

			 double p = random.NextDouble();

			 for (int i = 0; i < cumul.Length - 1; ++i)
				 if (p >= cumul[i] && p < cumul[i + 1])
					 return i;
			 throw new Exception("Failure to return valid city in NextCity");
		 }

		 double[] MoveProbs(int k, int cityX, bool[] visited, double[][] pheromones, double[][] dists)
		 {
			 // for ant k, located at nodeX, with visited[], return the prob of moving to each city
			 int numCities_ = pheromones[0].Length;
			 double[] taueta = new double[numCities_]; // inclues cityX and visited cities
			 double sum = 0.0; // sum of all tauetas
			 for (int i = 0; i < taueta.Length; ++i) // i is the adjacent city
			 {
				 if (i == cityX)
					 taueta[i] = 0.0; // prob of moving to self is 0
				 else if (visited[i] == true)
					 taueta[i] = 0.0; // prob of moving to a visited city is 0
				 else
				 {
					 taueta[i] = Math.Pow(pheromones[cityX][i], alpha) * Math.Pow((1.0 / distance(cityX, i, dists)), beta); // could be huge when pheromone[][] is big
					 if (taueta[i] < 0.00001)
						 taueta[i] = 0.00001;
					 else if (taueta[i] > (double.MaxValue / (numCities_ * 100)))
						 taueta[i] = double.MaxValue / (numCities_ * 100);
				 }
				 sum += taueta[i];
			 }

			 double[] probs = new double[numCities_];
			 for (int i = 0; i < probs.Length; ++i)
				 probs[i] = taueta[i] / sum; // big trouble if sum = 0.0
			 return probs;
		 }
		// --------------------------------------------------------------------------------------------


		 void UpdatePheromones(double[][] pheromones, int[][] ants, int[] demand, double[][] Rawdists)
		 {
			 for (int i = 0; i < pheromones.Length; ++i)
			 {
				 for (int j = i + 1; j < pheromones[i].Length; ++j)
				 {
					 for (int k = 0; k < ants.Length; ++k)
					 {
						 double length = Length(ants[k], demand, Rawdists); // length of ant k trail
						 double decrease = (1.0 - rho) * pheromones[i][j]  ;
						 double increase = 0.0;
						 if (EdgeInTrail(i + 1, j + 1, ants[k]) == true) increase =  Q / length;

						 pheromones[i][j] =  decrease + 2000000* increase;

						 if (pheromones[i][j] < 0.00001)
							 pheromones[i][j] = 0.00001;
						 else if (pheromones[i][j] > 1000.0)
							 pheromones[i][j] = 1000.0;

						 pheromones[j][i] = pheromones[i][j];
					 }
				 }
			 }
		 }

		 bool EdgeInTrail(int cityX, int cityY, int[] trail)
		 {
			 // are cityX and cityY adjacent to each other in trail[]?
			 int lastIndex = trail.Length - 1;
			 int idx = IndexOfTarget(trail, cityX);  // HERE IS THE PROBLEM

			 if (idx == 0 && trail[1] == cityY) return true;
			 else if (idx == 0 && trail[lastIndex] == cityY) return true;
			 else if (idx == 0) return false;
			 else if (idx == lastIndex && trail[lastIndex - 1] == cityY) return true;
			 else if (idx == lastIndex && trail[0] == cityY) return true;
			 else if (idx == lastIndex) return false;
			 else if (trail[idx - 1] == cityY) return true;
			 else if (trail[idx + 1] == cityY) return true;
			 else return false;
		 }

		// --------------------------------------------------------------------------------------------

		 double Distance(int cityX, int cityY, double[][] Rawdists) // Used for trail lenghts includes supplier returns
		 {
			 return Rawdists[cityX][cityY];
		 }



		 double distance(int cityX, int cityY, double[][] dists) // Used for pheromone update based on nearest city
		 {
			 return dists[cityX][cityY];
		 }

		// --------------------------------------------------------------------------------------------
 
	   
	}
}