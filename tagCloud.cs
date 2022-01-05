using System;
using System.IO;
using System.Collections.Generic;
//Pour le tri décroissant du dictionnaire par valeur
using System.Linq;
/*********************************************************************************************************
*	Version 4:
* 	Adapté au sujet 2				un nuage de mot par texte + un commun
* 	ajout de generateHtmlPortion()	support HTML: génère pour chaque fichier une portion d'HTML
*	ajout de fillTemplate()			support HTML: remplit le template avec les portions de code
* 	ajout de synthesisUpdate()		support HTML: créé un dictionnaire commun à tous les textes
* 									qui sera traité comme les autres dictionnaire
* 	inTextStopWords()				récupère les mots entièrement en majuscule pour les supprimer (utiles
* 									pour les pièces de théâtre)
*	modification de Main()			appelle la fonction sur tous les fichiers du répertoire "./txt"
* 									intègre les 4 fonctions précédentes
* 									donne l'état d'avancée de l'analyse
* 	modification de stopwords.txt	à partir des premiers résultats obtenus, modification manuelle
*********************************************************************************************************/

class nuageMots{

	static void Main(){
		//Récupérer tous les fichiers du dossier
		string[] paths=Directory.GetFiles("./txt");
		//Liste des parties du code HTML (une string par texte)
		List<string> htmlParts=new List<string>();
		//Dictionnaire commun à tous les textes (pour le nuage de texte commun)
		Dictionary<string,int> synthesis = new Dictionary<string,int>();
		//Pour chaque fichier texte
		foreach(string path in paths){
			//Récupération du nom du fichier, pour l'export
			string filename=Path.GetFileNameWithoutExtension(path);
			Console.WriteLine("Analyse de: " + filename);
			//lecture d'un fichiers du répertoire
			Console.WriteLine("Découpage du fichier...");
			string[] words=readFile(path,false);
			//Récupération de "mots" inutiles (Numéro de chapitre, livre, auteur, nom en majuscule dans une pièce de théâtre)
			string[] inTextemptyWords = inTextStopWords(words);
			//Tableau en minuscule
			words=words.Select(str => str.ToLower()).ToArray();
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
			stopWordsClear(occFinal, inTextemptyWords);
			//Mise à jour du dictionnaire commun
			synthesisUpdate(synthesis,occFinal);
			//Génération du code HTML pour ce texte
			Console.WriteLine("Génération du code HTML pour ce fichier\n");
			htmlParts.Add(generateHtmlPortion(occFinal,filename));
		}
		Console.WriteLine("Création du fichier HTML final");
		htmlParts.Add(generateHtmlPortion(synthesis,"Nuage de mot commun"));
		fillTemplate(htmlParts);
		Console.WriteLine("Fin du programme");
	}

	public static void fillTemplate(List<string> htmlParts){
		/*	fillTemplate:	proc
		*	Crée une copie du fichier de template et complète
		*	cette copie avec du code HTML
		*	param:	htmlParts:	List<string>:	liste de code pour chaque texte
		*	local:	author:	string:	"prénom nom" de l'auteur
		*			sr:	StreamReader:	parcourt le fichier de template
		*			sw:	SreamWriter:	ajoute le code récupéré par le StreamReader
		*								et le code contenu dans htmlParts
		*			line:	string:	string temporaire pour le parcours
		*			i:	int:	indice de parcours de htmlParts
		*	return:	void
		*/
		//Récupération de l'auteur
		Console.WriteLine("Entrer le Prénom, Nom de l'auteur:");
		string author = Console.ReadLine();
		//Création du fichier de sortie
		if(File.Exists("results.html"))
			File.Delete("results.html");
		StreamReader sr=File.OpenText("template.html");
		StreamWriter sw=File.CreateText("results.html");
		string line;
		//Recherche du premier point d'arrêt
		while( (line=sr.ReadLine())!="\t\t\t<!--INSERT AUTHOR-->"){
			sw.WriteLine(line);
		}
		//Insertion du premier titre
		if(isVowel(author.ToLower()[0])){
			sw.WriteLine("<h1>Analyse des textes d'"+author+"</h1>");
		}
		else{
			sw.WriteLine("<h1>Analyse des textes de "+author+"</h1>");
		}
		//Recherche du deuxième point d'arrêt
		while( (line=sr.ReadLine())!="\t\t\t\t<!--INSERT RESULTATS TEXTES-->"){
			sw.WriteLine(line);
		}
		//Insertion de toutes les parties du code HTML (sauf la dernière, la synthèse)
		int i;
		for(i=0; i<htmlParts.Count-1; i++){
			sw.WriteLine(htmlParts[i]);
		}
		//Recherche du dernier point d'arrêt
		while( (line=sr.ReadLine())!="\t\t\t\t<!--INSERT SYNTHESE TEXTES-->"){
			sw.WriteLine(line);
		}
		//Insertion du code HTML pour la partie synthèse
		sw.WriteLine(htmlParts[i]);
		//On termine le parcours (l'écriture des dernières lignes dans la copie)
		while( (line=sr.ReadLine())!=null){
			sw.WriteLine(line);
		}
		sr.Close();
		sw.Close();
	}

	public static string generateHtmlPortion(Dictionary<string,int> Xdict, string displayName){
		/*	generateHtmlPortion:	func:	strings
		*	Génère pour un dictionnaire d'occurence de mots, le code HTML associé
		*	param:	Xdict:	Dictionary<string,int>:	dictionnaire d'occurence
		*			displayName:	string:						nom du fichier, à afficher
		*	local:	outParts:		string:						code HTML pour un texte
		*			RandomParts:	List<string>:				Liste temporaire. On lui ajoute les
		*														les codes à la suite, mais il faut ensuite
		*														la trier de manière aléatoire, ou sinon
		*														le nuage de mot sera trié
		*			rnd:			Random:						Génère un nombre aléatoire pour le tri
		*			n:				int:						data-weight="n". Rang du mot
		*			kvp:			KeyValuePair<string,int>:	Paire clef-valeur temporaire pour le parcours par ordre décroissant
		*			temp:			var:						query dans laquelle on récupère les éléments de la liste triée aléatoirement
		*			str:			string:						string temporaire pour le parcours de temp. Ajoutée à OutParts à chaque tour de boucle
		*	return:	outParts:		string:						code HTML pour un texte
		*/
		string outParts = "<article>\n<header>\n<h2>"+displayName+"</h2>\n</header>\n<ul class=\"wordCloud\" data-show-value>\n";
		List<string> RandomParts = new List<string>();
		Random rnd = new Random();
		int n=1;
		foreach(KeyValuePair<string,int> kvp in Xdict.OrderByDescending(key => key.Value).Take(15)){
			RandomParts.Add("<li data-weight=\""+n+"\">"+kvp.Key+"</li>\n");
			n++;
		}
		//Query creation: "Tri" aléatoire de la liste, récupération dans une query
		var temp = RandomParts.OrderBy(x => rnd.Next(0,15));
		//Query execution 
		foreach(string str in temp){outParts+=str;}
		//Ajout de la fin du code HTML pour ce texte
		outParts+="</ul>\n</article>";
		return outParts;
	}

	public static void synthesisUpdate(Dictionary<string,int> synthesis, Dictionary<string,int> occFinal){
		/*	synthesisUpdate:	proc
		*	Modifie un dictionnaire en le fusionnant avec un autre
		*	param:	synthesis:	Dictionary<string,int>:		dictionnaire à modifier
		*			occFinal:	Dictionary<string,int>:		dictionnaire à fusionner avec synthesis
		*	local:	kvp:		KeyValuePair<string,int>:	Paire clef-valeur temporaire pour le parcours
		*/
		foreach(KeyValuePair<string,int> kvp in occFinal.OrderByDescending(key => key.Value)){
			if(synthesis.ContainsKey(kvp.Key)){
				synthesis[kvp.Key]+=kvp.Value;
			}
			else{
				synthesis.Add(kvp.Key,kvp.Value);
			}
		}
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
			//Selon les compilateurs la méthode Split requiert un char ou un char[]
			words = sr.ReadToEnd().Split(new char[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);
		else{
			char[] separators={char.Parse("'"),'%','*','°','–','0','1','2','3','4','5','6','7','8','9','\n','\r','\t','!','#','(',')',',','"','«','»','.','/',':',';','?','[',']','`',' ','-','’','“','”','„','…'};
			words = sr.ReadToEnd().Split(separators, StringSplitOptions.RemoveEmptyEntries);
		}
		sr.Close();
		return words;
	}

	public static string[] inTextStopWords(string[] Xtxt){
		/*	inTextStopWords:	proc
		*	Cherche dans le texte des mots qui n'apportent rien
		*	Pour une pièce de théâtre: Les mots entièrement en majuscule
		*	Ecris ces mots dans un fichier, qui sera utilisé par stopWordsClear()
		*	param:	Xtxt:	string[]:		tableau de mots
		*	local:	lst:	List<string>	stocke les mots qui sont en majuscules
		*			i:		int:			indice pour le parcours
		*/
		List<string> lst = new List<string>();
		for(int i=0; i< Xtxt.Length; i++){
			/*Si le mot est entièrement en majuscule*/
			if(Xtxt[i].ToUpper()==Xtxt[i]){
				lst.Add(Xtxt[i].ToLower());
			}
		}
		return lst.Distinct().ToArray();
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
		foreach(KeyValuePair<string,int> kvp in Xdict){
			Console.WriteLine(kvp.Key+": "+kvp.Value);
		}
	}
}

