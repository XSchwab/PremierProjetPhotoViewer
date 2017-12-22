#Ajout de métadonnées sur la météo
Le client souhaite ajouter une information concernant la météo sur chaque photo. Il nous demande une solution pour intégrer ceci à notre projet. 

Voici les solutions proposées :

- Utilisation d'un champ de métadonnées inutilisé
- Création d'un  nouveau champ de métadonnées 

##Création d'un  nouveau champ de métadonnées 
Cette solution doit être approfondis car sa faisabilité n'est pas certaine. Ils nous faudrait réaliser des recherches sur la possibilité de rajouter un champ à une image.

##Utilisation d'une métadonnées inutilisé
La solution la plus évidente est de se servir d'un champ de métadonnées inutilisé.

Pour ce faire, Voici les actions à effectuer. 

- Choisir un champ de métadonnées pertinent 
	- le champ objet pourrait faire l'affaire
- Rajouter la lecture dans la méthodes "GetTag"
- Rajout l'écriture dans la méthodes "AddTag"
- Ajouter le nouveau champ à la recherche
- Modifier le XAML en conséquences
	- Ajout du champ lecture et écriture de la météo

Ainsi la deuxième solution sera privilégiée pour la réalisation de la demande du client. L'architecture de notre application nous permet d'ajout rapidement cette fonctionnalité. Nous effectuerons tout de même quelques recherches sur la première solution dans le cas ou le client souhaiterais ajouter encore d'autre champ de métadonnées.


