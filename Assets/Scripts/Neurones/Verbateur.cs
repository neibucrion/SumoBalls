using System.Collections.Generic;
using UnityEngine;
using ChargeurCSV;

namespace NeuralNetwork
{
    public class Verbateur
    {
        public NeuralNet reseau;

        private List<List<double>> source;

        public Verbateur(NeuralNet res)
        {
            reseau = res;
        }

        public void sauvegardeReseau()
        {
            try
            {
                string dataToDump = recolteValeurs();
                System.IO.File.WriteAllText("sauvegardeReseau.csv", dataToDump);
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine("{0}\n", e.Message);
            }
        }

        private string recolteValeurs()
        {
            string donnees = "";
            donnees += ajouteDonneesLayer(reseau.InputLayer) + "\n";
            donnees += donneesHiddenLayer();
            donnees += ajouteDonneesLayer(reseau.OutputLayer);
            return donnees;
        }

        private string ajouteDonneesLayer(List<Neuron> layer)
        {
            string ligne = "";
            foreach (Neuron neuron in layer)
            {
                ligne += neuron.Bias.ToString();
                if (layer.IndexOf(neuron) != layer.Count - 1)
                {
                    ligne += ",";
                }
            }
            return ligne;
        }

        private string donneesHiddenLayer()
        {
            string dataHidden = "";
            foreach (Neuron neuron in reseau.HiddenLayer)
            {
                dataHidden += neuron.Bias.ToString() + ",";
                dataHidden += ajouteSynapse(neuron.InputSynapses);
                dataHidden += ajouteSynapse(neuron.OutputSynapses);
            }
            return dataHidden;
        }
        
        private string ajouteSynapse(List<Synapse> synapses)
        {
            string donneesSynapses = "";
            foreach (Synapse syn in synapses)
            {
                donneesSynapses += syn.Weight.ToString();
                if (synapses.IndexOf(syn) != synapses.Count - 1)
                {
                    donneesSynapses += ",";
                }
                else
                {
                    donneesSynapses += "\n";
                }
            }
            return donneesSynapses;
        }

        public void chargeReseau(List<List<double>> tab)
        {
            source = tab;
            if (verifieTailleTableau())
            {
                chargeLayer(reseau.InputLayer, source[0]);
                chargeHiddenLayer();
                chargeLayer(reseau.OutputLayer, source[source.Count - 1]);
            }
            else
            {
                Debug.Log("Taille de la sauvegarde non comptatible avec la taille du réseau : chargement impossible !");
            }
        }

        private bool verifieTailleTableau()
        {
            bool retour = false;
            int compteHiddenLayer = (reseau.HiddenLayer.Count * 2) + 2;
            if (source[0].Count == reseau.InputLayer.Count 
                && source.Count == compteHiddenLayer 
                && source[source.Count -1].Count == reseau.OutputLayer.Count)
            {
                retour = true;
            }
            return retour;
        }

        private void chargeLayer(List<Neuron> layer, List<double> liste)
        {
            for (int i = 0; i < layer.Count; i++)
            {
                layer[i].Bias = liste[i];
            }
        }

        private void chargeHiddenLayer()
        {
            for (int i = 0; i < reseau.HiddenLayer.Count; i++)
            {
                Neuron neuron = reseau.HiddenLayer[i];
                int indexDecale = (i * 2) + 1;
                neuron.Bias = source[indexDecale][0];
                List<double> resteLigne = source[indexDecale];
                resteLigne.RemoveAt(0);
                ajusteSynapses(neuron.InputSynapses, resteLigne);
                ajusteSynapses(neuron.OutputSynapses, source[indexDecale +1]);
            }
        }

        private void ajusteSynapses(List<Synapse> synapses, List<double> liste)
        {
            for (int i = 0; i < synapses.Count; i++)
            {
                synapses[i].Weight = liste[i];
            }
        }

    }
}