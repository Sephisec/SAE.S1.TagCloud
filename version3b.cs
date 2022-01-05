using System;
using System.IO;
using System.Collections.Generic;

class nuageMots{

	static void Main(){
		//Récupérer tous les fichiers
		string[] files=Directory.GetFiles("./txt");
		//lecture d'un des fichiers du répertoire
		string[] words=readFile(files[0],false);
		//Récupération des racines du fichier
		wordEndingSteps(words);
		//Construction du dictionnaire d'occurrences
		Dictionary<string,int> occurrences = dictBuild(words);
		//Suppression des mots "inutiles"
		stopWordsClear(occurrences);
		//Affichage du dictionnaire
		dictDisplay(occurrences);
		patternTest();
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
			char[] separators={char.Parse("'"),'%','*','°','–','0','1','2','3','4','5','6','7','8','9','\n','\r','\t','!','#','(',')',',','"','«','»','.','/',':',';','?','[',']','`',' ','-','’','“','”','„','…'};
			words = sr.ReadToEnd().ToLower().Split(separators, StringSplitOptions.RemoveEmptyEntries);
		}
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
			//Vérification des terminaisons
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

	public static void patternTest(){
		/*	patternTest:	proc
		*	Tests sur la fonction de
		*	découpage wordPattern()
		*	local:	array:	string[]:	tableau de mots à tester
		*			str:	string:		string temporaire pour le parcours
		*/
		string[] array = {"Magnific","mange", "mignon", "arbre", "alphabet", "rempl","t"};
		Console.WriteLine("TEST =======================================================\n");
		foreach(string str in array){
			Console.WriteLine("Test sur "+str);
			Console.WriteLine(wordPattern(str));
		}
	}
}