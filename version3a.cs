using System;
using System.IO;
using System.Collections.Generic;

class nuageMots{

	static void Main(){
		//Récupérer tous les fichiers
		string[] files=Directory.GetFiles("./txt");
		//lecture d'un des fichiers du répertoire
		string[] words=readFile(files[1],false);
		//Récupération des racines du fichier
		wordEndingSteps(words);
		//Construction du dictionnaire d'occurrences
		Dictionary<string,int> occurrences=dictBuild(words);
		//Suppression des mots "inutiles"
		stopWordsClear(occurrences);
		occurrences.Remove("");
		//Affichage du dictionnaire
		dictDisplay(occurrences);
	}

	public static string[] readFile(string path, bool byLine){
		/*	readFile:	func:	string[]
		*	Renvoie un tableau à partir de strings
		*	contenus dans un fichier, en supprimant
		*	les caractères de ponctuation et en
		*	modifiant la casse
		*	param:	path:		string:			chemin d'accès du fichier à ouvrir
		*			byLine:		bool:			(v3a): indique qu'on veut un tableau de lignes
		*	local:	sr:			StreamReader:	permet d'accéder au contenu du fichier
		*			separators: char[]:			tableau des caractères de ponctuation
		*	return:				string[]:		tableau des string en minuscule sans ponctuation
		*	[Note]:	StringSplitOptions.RemoveEmptyEntries:	permet de supprimer les string vides
		*/
		StreamReader sr;
		sr = File.OpenText(path);
		if(byLine)
			return sr.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries);
		char[] separators={'\n','\r','!','#','(',')',',','"','«','»','.','/',':',';','?','[',']','`',' ','-','’'};
		return sr.ReadToEnd().ToLower().Split(separators, StringSplitOptions.RemoveEmptyEntries);
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
		return occurrences;
	}


	public static void stopWordsClear(Dictionary<string,int> Xdict){
		/*	stopWordsClear:	proc
		*	Supprime les mots vides du dictionnaire d'occurences
		*	param:	Xdict:		Dictionary<string,int>:	dictionnaire d'occurrences
		*	local:	stopWords:	string[]:				tableau de mots "vides"
		*			stopWord:	string:					string temporaire pour le parcours
		*/
		string[] stopWords = readFile("./asset/stopwords.txt",false);
		foreach(string stopWord in stopWords){
			if(Xdict.ContainsKey(stopWord))
				Xdict.Remove(stopWord);
		}
	}

	public static void wordEndingSteps(string[] XWords){
		/*	wordEndingSteps:	proc
		*	Appelle la fonction de suppression de terminaisons
		*	sur le tableau de mots avec les fichiers des 3 étapes
		*	param:	XWords:	string[]:	tableau de mots
		*	local:	paths:	string[]:	tableau de chemins d'accès
		*			path:	string:		string temporaire pour le parcours
		*/
		string[] paths={"etape1.txt","etape2.txt","etape3.txt"};
		foreach(string path in paths){
			wordEndingClear(XWords,"./asset/"+path);
		}
	}

	public static void wordEndingClear(string[] XWords, string path){
		/*	wordEndingClear:	proc
		*	Modifie le tableau de mots pour ne conserver pour chaque
		*	mot que le radical (en supprimant les terminaisons issues
		*	du fichier renseigné)
		*	param:	XWords:		string[]:		tableau de mots
		*			path:		string:			chemin d'accès du fichier de terminaisons
		*	local:	wordEndingFile:	string[]:	tableau de string contenant les lignes du fichier de terminaisons
		*			processed:	bool:	indique si le mot a déjà été traité
		*			wordEnding:	string[]:		ligne du fichier de terminaison splitée sur les espaces
		*			wordLen:	int:			Longueur du mot
		*			endingLen:	int:			Longueur de la terminaison
		*/
		string[] wordEndingFile = readFile(path,true);
		for(int i=0; i<XWords.Length; i++){
			bool processed=false;
			for(int j=0; j<wordEndingFile.Length && !processed; j++){
				string[] wordEnding = wordEndingFile[j].Split(' ');
				if(EndsWith(XWords[i],wordEnding[1])){//Fonction RECODEE!!!
					int wordLen = XWords[i].Length;
					int endingLen = wordEnding[1].Length;
					if(wordEnding[2]=="epsilon"){
						XWords[i]=XWords[i].Substring(0, wordLen-endingLen);
					}
					else{
						XWords[i]=XWords[i].Substring(0, wordLen-endingLen)+wordEnding[2];
					}
					processed=true;
				}
			}
		}
	}

	public static bool EndsWith(string mot, string term){
		/*	EndsWith: func: bool
		*	Réimplantation de la méthode native EndsWith()
		*	Indique si une string se termine par une autre string
		*	param:	mot:	string:	mot dans lequel on cherche une terminaison
		*			term:	string:	terminaison à chercher
		*	local:	iStart:	int:	indice à partir duquel on commence le parcours
		*							du mot. S'il est négatif, cela signifie que le
		*							mot est plus court que la terminaison.
		*	return: bool
		*/
		int iStart=mot.Length-term.Length;
		//Si le mot est plus court que la terminaison à chercher
		if(iStart<0)
			return false;
		for(int i=0;i < term.Length; i++){
			//Si les caractères sont différents
			if(mot[iStart+i]!=term[i])
				return false;
		}
		//Si mot se termine par term
		return true;
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