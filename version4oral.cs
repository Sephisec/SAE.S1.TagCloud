using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
/**********************************************************************************************
*	Version 4:
*	modification de Main()			appelle la fonction sur tous les fichiers du répertoire "./txt"
* 									donne l'état d'avancée de l'analyse
*	modification de stopWordsClear	prend en param un tableau et non plus un path
* 	ajout de wordModify()			simplifie la fonction wordEndingClear
*	ajout de archivage()			simplifie la fonction wordEndingSteps
**********************************************************************************************/

class nuageMots{

	static void Main(){
		//Récupérer tous les fichiers du dossier
		string[] paths=Directory.GetFiles("./txt");
		//Pour chaque fichier texte
		foreach(string path in paths){
			//Récupération du nom du fichier, pour l'export
			string filename=Path.GetFileNameWithoutExtension(path);
			Console.WriteLine("Analyse de:" + filename);
			//lecture d'un fichiers du répertoire
			Console.WriteLine("Découpage du fichier...");
			string[] words=readFile(path,false);
			//Récupération des racines du fichier + Liste des historiques de chaque mot
			Console.WriteLine("Récupération des radicaux de "+words.Length+" mots");
			List<List<string>> wordRoots = wordEndingSteps(words);
			// Construction du dictionnaire d'occurrences des racines et des mots de base
			Console.WriteLine("Construction du dictionnaire d'occurences des racines");
			Dictionary<string,Dictionary<string,int>> occDetailed = dictBuildStep1(wordRoots);
			//Construction du dictionnaire final
			Console.WriteLine("Construction du dictionnaire final");
			Dictionary<string,int> occFinal = dictBuildStep2(occDetailed);
			//Suppression des mots "inutiles"
			string[] emptyWords = readFile("./asset/stopwords.txt", false);
			stopWordsClear(occFinal, emptyWords);
			//Affichage du dictionnaire
			exportResults(filename,occDetailed, occFinal);
		}
		Console.WriteLine("Fin du programme");
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
		*	param:	Xdict:			Dictionary<string,Dictionary<string,int>>:		dictionnaire de racines, de mots de base
		*																			et de nombres d'occurrences
		*
		*	local:	occurrences2:	Dictionary<string,int>:							dictionnaire de mots (en tenant compte
		*																			des racines) et du nombre d'occurences
		*
		*			kvp1:			KeyValuePair<string,Dictionary<string,int>>:	Paire clef-valeur pour le parcours
		*
		*			totOcc:			int:											nombre total d'occurence d'une racine (on le calcule
		*																			en additonnant le nb d'occurence des mots de base)
		*
		*			maxOcc:			int:											nombre d'occurence du mot le plus
		*																			récurrent pour une racine
		*
		*			maxStr:			string:											mot de base le plus récurrent pour une racine
		*																			(on l'obtient en grâce à maxOcc)
		*
		*	return:	occurrences2:	Dictionary<string,int>:							dictionnaire de mots (en tenant compte
		*																			des racines) et du nombre d'occurences
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
			words = sr.ReadToEnd().ToLower().Split(new char[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);
		else{
			char[] separators={char.Parse("'"),'%','*','°','–','0','1','2','3','4','5','6','7','8','9','\n','\r','\t','!','#','(',')',',','"','«','»','.','/',':',';','?','[',']','`',' ','-','—','’','“','”','„','…'};
			words = sr.ReadToEnd().ToLower().Split(separators, StringSplitOptions.RemoveEmptyEntries);
		}
		sr.Close();
		return words;
	}

	public static void stopWordsClear(Dictionary<string,int> Xdict, string[] stopWords){
		/*	stopWordsClear:	proc
		*	Supprime les mots vides du dictionnaire d'occurences
		*	param:	Xdict:		Dictionary<string,int>:	dictionnaire trié d'occurrences
		*			stopWords:	string[]:						tableau de mots "vides"
		*	local:	stopWord:	string:							string temporaire pour le parcours
		*/
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
		*											[DEBUG]: Possible d'archiver les étapes successives si
		*											on met la fonction d'ajout dans le foreach
		*			word:		string:				string temporaire pour le parcours
		*			path:		string:				string temporaire pour le parcours
		*	return:	archive:	List<List<string>>:	stocke le mot de base et le radical final
		*/
		string[] paths={"etape1.txt","etape2.txt","etape3.txt"};
		List<List<string>> archive = new List<List<string>>();
		//Archivage du mot de base
		archivage(archive,XWords);
		foreach(string path in paths){
			XWords=wordEndingClear(XWords,"./asset/"+path);
		}
		//Archivage du radical final
		archivage(archive,XWords);
		return archive;
	}

	public static void archivage(List<List<string>> archive, string[] XWords){
		/*	archivage:	proc
		*	Allège la fonction wordEndingSteps
		*	Archive le mot de base (1ère condition) ou le radical après suppresion
		*	de la terminaison (2e condition). On peut donc garder les radicaux après
		*	chaque étape, si on appelle cette fonction dans la boucle de wordEndingSteps().
		*	param:	archive:	List<List<string>>:	liste avec pour chaque terminaison un mot de base
		*			XWords:		string[]:	tableau de mot du texte
		*
		*/
		if(archive.Count==0){
			foreach(string word in XWords)
				archive.Add(new List<string>{word});
		}
		else{
			for(int i=0; i<XWords.Length;i++){
				archive[i].Add(XWords[i]);
			}
		}

	}

	public static string[] wordEndingClear(string[] XWords, string path){
		/*	wordEndingClear:	proc
		*	Modifie le tableau de mots pour ne conserver pour chaque
		*	mot que le radical (en supprimant les terminaisons issues
		*	du fichier renseigné)
		*	param:	XWords:			string[]:		tableau de mots
		*			path:			string:			chemin d'accès du fichier de terminaisons
		*	local:	wordEndingFile:	string[]:		tableau de string contenant les lignes du fichier de terminaisons
		*			processed:		bool:			indique si le mot a déjà été traité
		*			wordEnding:		string[]:		ligne du fichier de terminaison splitée sur les espaces
		*/
		string[] wordEndingFile = readFile(path,true);
		//Parcours mot par mot dans le texte
		for(int i=0; i<XWords.Length; i++){
			bool processed=false;
			//Parcours du tableau de terminaison
			for(int j=0; j<wordEndingFile.Length && !processed; j++){
				string[] wordEnding = wordEndingFile[j].Split(' ');
				string baseword=XWords[i];
				string terminaison=wordEnding[1];
				//Si le mot contient la terminaison
				if(EndsWith(baseword,terminaison)){//Fonction RECODEE!!!
					XWords[i]=wordModify(baseword, terminaison, wordEnding[2], int.Parse(wordEnding[0]));
					processed=true;
				}
			}
		}
		return XWords;
	}

	public static string wordModify(string baseword, string terminaison, string replace, int m){
		/*	wordModify:	func:	string
		*	Modifie un mot en remplaçant sa terminaison ou en
		*	la supprimant
		*	param:	baseword:	string:	mot à modifier
		*			terminaison:	string:	terminaison à supprimer
		*			replace:	string:	remplacement de la terminaison
		*			m:	int:	critère de validité de la terminaison
		*	local:	wordLen:	int:	longueur de baseword
		*			endingLen:	int:	longueur de la terminaison
		*			tempWord:	string:	mot après modification de la terminaison
		*								(en attente de la vérification du critère de validité)
		*	return:	baseword:	string:	mot modifié (ou pas, selon le critère de validité)
		*/
		int wordLen=baseword.Length;
		int endingLen=terminaison.Length;
		string tempWord;
		//Cas n°1: suppression de la terminaison
		if(replace=="epsilon"){
			replace="";
		}
		tempWord=baseword.Substring(0, wordLen-endingLen)+replace;
		//Vérification de la validité du radical
		if(wordPattern(tempWord)>m){
			return tempWord;
		}
		return baseword;
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
		*	param:	Xdict:	Dictionary<string,int>:	dictionnaire trié de string et int
		*	local:	kvp:	KeyValuePair<string,int>:		Paire clefs-valeurs pour le parcours
		*/
		var sample = Xdict.OrderByDescending(key => key.Value).Take(15);
		foreach(KeyValuePair<string,int> kvp in sample){
			Console.WriteLine(kvp.Key+": "+kvp.Value);
		}
	}

	public static void exportResults(string filename, Dictionary<string,Dictionary<string,int>> occDetailed, Dictionary<string,int> occFinal){
		if(File.Exists("./results/"+filename+"OUTPUT.txt"))
			File.Delete("./results/"+filename+"OUTPUT.txt");
		StreamWriter sw = File.CreateText("./results/"+filename+"OUTPUT.txt");
		sw.WriteLine("Pour chaque racine, les mots qui ont permis d'arriver à celle-ci\n\n\n");
		foreach(KeyValuePair<string,Dictionary<string,int>> kvp1 in occDetailed){
			sw.WriteLine("\nradical:\t"+kvp1.Key+"\nbaseword(s):");
			foreach(KeyValuePair<string,int> kvp2 in kvp1.Value){
				sw.WriteLine(kvp2.Key+"\t"+kvp2.Value+" fois");
			}
		}
		sw.WriteLine("\n\n\nLe dictionnaire final (3c)\n\n\n");
		foreach(KeyValuePair<string,int> kvp in occFinal.OrderByDescending(x => x.Value)){
			sw.WriteLine(kvp.Key+": "+kvp.Value);
		}
	}
}

