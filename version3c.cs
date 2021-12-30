using System;
using System.IO;
using System.Collections.Generic;

class nuageMots{

	static void Main(){
		//Récupérer tous les fichiers
		string[] files=Directory.GetFiles("./txt");
		//lecture d'un des fichiers du répertoire
		string[] words=readFile(files[2],false);
		//Récupération des racines du fichier + Liste des historiques de chaque mot
		List<List<string>> wordRoots = wordEndingSteps(words);
		// Construction du dictionnaire d'occurrences des racines et des mots de base
		Dictionary<string,Dictionary<string,int>> occDetailed = dictBuildStep1(wordRoots);
		//Construction du dictionnaire final
		Dictionary<string,int> occFinal = dictBuildStep2(occDetailed);
		//Suppression des mots "inutiles"
		stopWordsClear(occFinal);
		//Affichage du dictionnaire
		dictDisplay(occFinal);
	}

	public static Dictionary<string,Dictionary<string,int>> dictBuildStep1(List<List<string>> Xlist){
		/*	dictBuildStep1:	func:	Dictionary<string,Dictionary<string,int>>
		*	Renvoie un dictionnaire de racines avec les mots de base qui ont
		*	permis d'arriver à ces racines, ainsi que leurs nombres d'occurences
		*	Dict sous la forme Dictionary<radical,Dictionary<baseword,nbOccurence>>
		*	param:	Xlist:		List<List<string>>:							Archive des mots de bases et des radicaux
		*	local:	occurences:	Dictionary<string,Dictionary<string,int>>:	Dictionnaire de racine, mot de base et occurence
		*			lst:		List<string>:								liste temporaire pour le parcours
		*			baseWord:	string:										récupération du mot de base dans la liste (plus lisible)
		*			radical:	string:										récupération de la racine dans la liste (plus lisible)
		*			tempDict:	Dictionary<string,int>:						dictionnaire temporaire pour l'ajout au dictionnaire d'occurence
		*	return:	occurences:	Dictionary<string,Dictionary<string,int>>:	Dictionnaire de racine, mot de base et occurence
		*/
		Dictionary<string,Dictionary<string,int>> occurrences = new Dictionary<string,Dictionary<string,int>>();
		foreach(List<string> lst in Xlist){
			string baseWord=lst[0];
			string radical=lst[1];
			//Cas où le dico contient le radical final
			if(occurrences.ContainsKey(radical)){
				//Si pour ce radical on a le même mot de base que celui en cours (on doit incrémenter)
				if(occurrences[radical].ContainsKey(baseWord))
					occurrences[radical][baseWord]++;
				//Sinon on ajoute le mot au dict avec un nombre d'occurence à 1
				else
					occurrences[radical].Add(baseWord,1);
			}
			//Cas où on doit ajouter le radical et son mot de base au dico
			else{
				Dictionary<string,int> tempDict = new Dictionary<string,int>();
				tempDict.Add(baseWord,1);
				occurrences.Add(radical,tempDict);
			}
		}
		return occurrences;
	}

	public static Dictionary<string,int> dictBuildStep2(Dictionary<string,Dictionary<string,int>> Xdict){
		/*	dictBuildStep2:	func:	Dictionary<string,int>
		*	A partir d'un dictionnaire de racines, de mots de base et de nombres
		*	d'occurrences, construit un dictionnaire Dictionary<mot,nombreDOccurrence>.
		*	en prenant pour chaque racine le mot le plus récurrent
		*	param:	Xdict:	Dictionary<string,Dictionary<string,int>>:				dictionnaire de racines, de mots de base et de nombres d'occurrences
		*	local:	occurrences2:	Dictionary<string,int>:							dictionnaire de mots (en tenant compte des racines) et du nombre d'occurences
		*			kvp1:			KeyValuePair<string,Dictionary<string,int>>:	Paire clef-valeur pour le parcours
		*			totOcc:			int:											nombre total d'occurence d'une racine (on le calcule en additonnant le nb d'occurence des mots de base)
		*			maxOcc:			int:											nombre d'occurence du mot le plus récurrent pour une racine
		*			maxStr:			string:											mot de base le plus récurrent pour une racine (on l'obtient en grâce à maxOcc)
		*	return:	occurrences2:	Dictionary<string,int>:							dictionnaire de mots (en tenant compte des racines) et du nombre d'occurences
		*/
		Dictionary<string,int> occurrences2 = new Dictionary<string,int>();
		//Parcours racine par racine
		foreach(KeyValuePair<string,Dictionary<string,int>> kvp1 in Xdict){
			//Parcours des association baseword-occurences
			int totOcc=0;
			int maxOcc=0;
			string maxStr="";
			foreach(KeyValuePair<string,int> kvp2 in kvp1.Value){
				//Recherche pour une racine le mot de base le plus récurrent
				if(kvp2.Value>maxOcc){
					maxOcc=kvp2.Value;
					maxStr=kvp2.Key;
				}
				totOcc+=kvp2.Value;
			}
			occurrences2.Add(maxStr,totOcc);
		}
		return occurrences2;
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
		*			words:		string[]:		tableau des string
		*			separators: char[]:			tableau des caractères de ponctuation
		*	return:	words:		string[]:		tableau des string
		*	[Note]:	StringSplitOptions.RemoveEmptyEntries:	permet de supprimer les string vides
		*/
		StreamReader sr=File.OpenText(path);
		string[] words;
		if(byLine)
			words = sr.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries);
		else{
			char[] separators={'\n','\r','!','#','(',')',',','"','«','»','.','/',':',';','?','[',']','`',' ','-','’'};
			words = sr.ReadToEnd().ToLower().Split(separators, StringSplitOptions.RemoveEmptyEntries);
		}
		sr.Close();
		return words;
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

	public static List<List<string>> wordEndingSteps(string[] XWords){
		/*	wordEndingSteps:	func:	List<List<string>>
		*	Appelle la fonction de suppression de terminaisons
		*	sur le tableau de mots avec les fichiers des 3 étapes
		*	et (v3c) archive le mot de base et le radical final
		*	dans une liste.
		*	param:	XWords:		string[]:			tableau de mots
		*	local:	paths:		string[]:			tableau de chemins d'accès
		*			archive:	List<List<string>>:	stocke le mot de base et le radical final
		*											[DEBUG]: Possible d'archiver les étapes successives si on met la fonction d'ajout dans le foreach
		*			word:		string:				string temporaire pour le parcours
		*			path:		string:				string temporaire pour le parcours
		*	return:	archive:	List<List<string>>:	stocke le mot de base et le radical final
		*/
		string[] paths={"etape1.txt","etape2.txt","etape3.txt"};
		List<List<string>> archive = new List<List<string>>();
		//Archivage du mot de base
		foreach(string word in XWords){
			archive.Add(new List<string>{word});
		}
		foreach(string path in paths){
			XWords=wordEndingClear(XWords,"./asset/"+path);
		}
		//Archivage du radical final
		for(int i=0; i<XWords.Length;i++){
			archive[i].Add(XWords[i]);
		}
		return archive;
	}

	public static string[] wordEndingClear(string[] XWords, string path){
		/*	wordEndingClear:	proc
		*	Modifie le tableau de mots pour ne conserver pour chaque
		*	mot que le radical (en supprimant les terminaisons issues
		*	du fichier renseigné)
		*	param:	XWords:			string[]:		tableau de mots
		*			path:			string:			chemin d'accès du fichier de terminaisons
		*	local:	wordEndingFile:	string[]:		tableau de string contenant les lignes du fichier de terminaisons
		*			tempWord:		string:			(v3b): mot temporaire avant vérification de sa validité
		*			processed:		bool:			indique si le mot a déjà été traité
		*			wordEnding:		string[]:		ligne du fichier de terminaison splitée sur les espaces
		*			wordLen:		int:			Longueur du mot
		*			endingLen:		int:			Longueur de la terminaison
		*/
		string[] wordEndingFile = readFile(path,true);
		string tempWord;
		//Parcours mot par mot dans le texte
		for(int i=0; i<XWords.Length; i++){
			bool processed=false;
			//Parcours du tableau de terminaison
			for(int j=0; j<wordEndingFile.Length && !processed; j++){
				string[] wordEnding = wordEndingFile[j].Split(' ');
				//Si le mot contient la terminaison
				if(EndsWith(XWords[i],wordEnding[1])){//Fonction RECODEE!!!
					int wordLen = XWords[i].Length;
					int endingLen = wordEnding[1].Length;
					//Cas n°1: suppression de la terminaison
					if(wordEnding[2]=="epsilon"){
						tempWord=XWords[i].Substring(0, wordLen-endingLen);
						//Vérification de la validité du radical
						if(wordPattern(tempWord)>int.Parse(wordEnding[0])){
							XWords[i]=tempWord;
						}
					}
					//Cas n°2: remplacement de la terminaison
					else{
						tempWord=XWords[i].Substring(0, wordLen-endingLen)+wordEnding[2];
						//Vérification de la validité du radical
						if(wordPattern(tempWord)>int.Parse(wordEnding[0])){
							XWords[i]=tempWord;
						}
					}
					processed=true;
				}
			}
		}
		return XWords;
	}

	public static bool isVowel(char c){
		/*	isVowel:	func:	bool
		*	Renvoie true si un caractère est
		*	une voyelle et false si c'est un
		*	autre type de caractère
		*	param:	c:		char:	caractère à vérifier
		*	local:	vowels:	string:	string de voyelles
		*			v:		char:	char temporaire pour le parcours
		*	return:			bool
		*/
		string vowels="aeiouy";
		foreach(char v in vowels){
			if(c==v)
				return true;
		}
		return false;
	}

	public static int wordPattern(string word){
		/*	wordPattern:	func:	int
		*	Renvoie le nombre d'alternance voyelles-
		*	consonnes (VC) d'un mot
		*	param:	word:	string:	mot à analyser
		*	local:	i:		int:	indice pour le parcours du mot
		*			index:	int:	indice de sauvegarde de 
		*							la dernièrevaleur de i
		*			m:		int:	nombre de (VC)
		*	return:	m:		int:	nombre de (VC)
		*	[Note]:	On peut utiliser la liste pattern pour avoir
		*	le découpage du mot
		*/
		//List<string> pattern = new List<string>();
		int i=0;
		int index=0;
		int m=0;
		//[C]
		while(i<word.Length && !isVowel(word[index+i]))
			i++;
		//pattern.Add(word.Substring(index,i));
		index+=i;
		i=0;
		//(VC)
		while((index+i)<word.Length)
		{
			//Recherche de la prochaine consonne
			while((index+i)<word.Length && isVowel(word[index+i]))
				i++;
			//Si on est dans le cas de [V], on doit décrémenter m
			if((index+i)>=word.Length)
				m--;
			//Recherche de la prochaine voyelle
			while((index+i)<word.Length && !isVowel(word[index + i]))
				i++;
			//pattern.Add(word.Substring(index,i));
			index+=i;
			i=0;
			m++;
		}
		return m;
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
		*	return: 		bool
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
		*	[DEBUG]	Affiche un tableau de string
		*	param:	Xarray:	string[]:	tableau à afficher
		*	local:	word:	string:		string temporaire pour le parcours
		*/
		foreach(string word in Xarray)
			Console.WriteLine(word);
	}

	public static void lstLstStrDisplay(List<List<string>> XList){
		/*	lstLstStrDisplay:	proc
		*	[DEBUG]	Affiche une liste de liste de string
		*	param:	XList:	List<List<string>>:	liste de liste de string à afficher
		*	local:	lst:	List<string>:	liste temporaire pour le parcours
		*			str:	string:	string temporaire pour le parcours
		*/
		foreach(List<string> lst in XList){
			foreach(string str in lst){
				Console.Write(str+"\t\t");
			}
			Console.WriteLine();
		}
	}

	public static void dictDictDisplay(Dictionary<string,Dictionary<string,int>> Xdict){
		/*	lstLstStrDisplay:	proc
		*	[DEBUG]	Affiche un dictionnaire de dictionnaire de string-int
		*	param:	Xdict:	Dictionary<string,Dictionary<string,int>>:	dictionnaire à afficher
		*	local:	kvp1:	KeyValuePair<string,Dictionary<string,int>>:	Paire clefs-valeurs pour le parcours
		*			kvp2:	KeyValuePair<string,int>:	Paire clefs-valeurs pour le parcours
		*/
		foreach(KeyValuePair<string,Dictionary<string,int>> kvp1 in Xdict){
			Console.WriteLine("radical:\t"+kvp1.Key);
			foreach(KeyValuePair<string,int> kvp2 in kvp1.Value){
				Console.WriteLine(kvp2.Key+"\t"+kvp2.Value);
			}
		}
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