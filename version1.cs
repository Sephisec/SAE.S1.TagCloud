using System;
using System.IO;
using System.Collections.Generic;

class nuageMots{

	static void Main(){
		//Récupérer tous les fichiers
		string[] files=Directory.GetFiles("./txt");
		//lecture d'un des fichiers du répertoire
		string[] words=readFile(files[0]);
		//Construction du dictionnaire d'occurrences
		Dictionary<string,int> occurrences = dictBuild(words);
		//Affichage du dictionnaire
		dictDisplay(occurrences);
	}

	public static string[] readFile(string path){
		/*	readFile:	func:	string[]
		*	Renvoie un tableau à partir de mots
		*	contenus dans un fichier, en supprimant
		*	les caractères de ponctuation et en
		*	modifiant la casse
		*	param:	path:		string:			chemin d'accès du fichier à ouvrir
		*	local:	sr:			StreamReader:	permet d'accéder au contenu du fichier
		*			words:		string[]:		tableau des mots en minuscule sans ponctuation
		*			separators: char[]:			tableau des caractères de ponctuation
		*	return: words:		string[]:		tableau des mots en minuscule sans ponctuation
		*/
		StreamReader sr;
		string[] words;
		char[] separators={'\n','\r','!','#','(',')',',','"','«','»','.','/',':',';','?','[',']','`',' ','-','’'};
		sr = File.OpenText(path);
		words=sr.ReadToEnd().ToLower().Split(separators);
		sr.Close();
		return words;
	}

	public static Dictionary<string,int> dictBuild(string[] words){
		/*	dictBuild:	func:	Dictionary<string,int>
		*	Renvoie un dictionnaire de mots à partir d'un tableau
		*	avec pour chaque mot son nombre d'occurence dans
		*	le tableau
		*	param:	words:			string[]:				tableau de mots
		*	local:	occurrences:	Dictionary<string,int>:	dictionnaire d'occurrences
		*			word:			string:					string temporaire pour le parcours
		*	return:	occurrences: 	Dictionary<string,int>:	dictionnaire d'occurrences
		*/
		Dictionary<string,int> occurrences = new Dictionary<string,int>();
		foreach(string word in words){
			if(occurrences.ContainsKey(word))
				occurrences[word]++;
			else
				occurrences.Add(word,1);
		}
		//Suppression de la string vide
		occurrences.Remove("");
		return occurrences;
	}

	public static void arrayDisplay(string[] Xarray){
		/*	arrayDisplay:	proc
		*	[DEBUG] Affiche un tableau de string
		*	param:	Xarray:	string[]:	tableau à afficher
		*	local:	word:	string:		string temporaire pour le parcours
		*/
		foreach(string word in Xarray)
			Console.WriteLine(word);
	}

	public static void dictDisplay(Dictionary<string,int> Xdict){
		/*	dictDisplay:	proc
		*	Affiche un dictionnaire
		*	param:	Xdict:	Dictionary<string,int>:		dictionnaire de string et int
		*	local:	kvp:	KeyValuePair<string,int>:	Paire clefs-valeurs pour le parcours
		*/
		foreach(KeyValuePair<string,int> kvp in Xdict){
			Console.WriteLine(kvp.Key+": "+kvp.Value);
		}
	}
}