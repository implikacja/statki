#include <sys/types.h>
#include <sys/wait.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <unistd.h>
#include <netdb.h>
#include <signal.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#include <pthread.h>
#include <string>
#include <iostream>
#include <cstdlib>
using namespace std;

#define SERVER_PORT 1234
#define GAME_NUMBER 2
#define BUF_SIZE 1024

pthread_mutex_t mutex = PTHREAD_MUTEX_INITIALIZER;
int clients = 0;
//struktura zawieraj¹ca dane, które zostan¹ przekazane do w¹tku
struct thread_data_t
{
	int socket;
	int id;//miejsce w tablicy players
};
//struktura przechowuj¹ca informacje o grze
struct information
{
	int myFields[100];//0 pole puste -1 pole trafione 1-10 nr statku, którego maszt znajduje siê na tym polu
	string Username;
	int shipsSinked;
	int shipNumber;
	int gameFaze;//0-powitanie 1-przesyl statków 2-gra+wynik
};

int players[GAME_NUMBER*2];//sockety pod³¹czonych graczy

information info[GAME_NUMBER*2];

bool SendMessage(string message, int playerSocket)
{
	message = message + '&';
	char* cmsg = new char[message.length() + 1];
	strcpy(cmsg, message.c_str());
	write(playerSocket, cmsg, strlen(cmsg));
	delete[] cmsg;
	return true;
}

string GetMessage(int playerSocket)
{
	int k = 0;
	string allData = "";
	bool received = false;
	while(!received)
	{
		char message[BUF_SIZE];
		bzero(message, BUF_SIZE);
		k = read(playerSocket, message, sizeof(message));
		string data(message);
		allData += data;
		if (allData[allData.size() - 1]=='&')
			received = true;
	}
	allData = allData.substr(0, allData.size() - 1);
	return allData;


}


void *ThreadBehavior(void *t_data)
{
    thread_data_t *th_data = (struct thread_data_t*)t_data;
	bool EndGame = false;
	int id = (*th_data).id;
	int enemyId;
	if (id % 2 == 0)enemyId = id + 1;
	else enemyId = id - 1;
	pthread_mutex_lock(&mutex);
	info[id].gameFaze = 0;
	pthread_mutex_unlock(&mutex);
	while(!EndGame)
	{
		string msg = GetMessage((*th_data).socket);
		cout << "Odbieram " << id << " " << msg<<endl;

		if (msg.substr(0, 2) == "hi" && info[id].gameFaze == 0)
		{
			pthread_mutex_lock(&mutex);
				if (info[id].Username.size()==0)
					info[id].Username = msg.substr(2);
					info[id].gameFaze = 1;
					if ((info[enemyId].Username).size() > 0)
					{
						string answer = "hi" + info[enemyId].Username;
						SendMessage(answer, players[id]);
						SendMessage("hi" + info[id].Username, players[enemyId]);
						for (int i = 0; i < 100; i++)
						{
							info[id].myFields[i] = 0;
							info[enemyId].myFields[i] = 0;
							info[id].shipNumber = 1;
							info[enemyId].shipNumber = 1;
						}
							
					}
			pthread_mutex_unlock(&mutex);
		}
		else if(msg.substr(0,4) == "ship" && info[id].gameFaze == 1)
		{
			pthread_mutex_lock(&mutex);
			msg.erase(msg.begin(), msg.begin()+4);
			for (int j = 0; j < msg.size(); j += 2)
			{
				int coord = atoi(msg.substr(j,2).c_str());
				info[id].myFields[coord] = info[id].shipNumber;
			}
			SendMessage("ok", players[id]);
			info[id].shipNumber++;
			if (info[id].shipNumber == 11 && info[enemyId].shipNumber == 11)
			{
				if (id % 2 == 0)
				{
					SendMessage("first", players[id]);
					SendMessage("second", players[enemyId]);
				}
				else
				{
					SendMessage("second", players[id]);
					SendMessage("first", players[enemyId]);

				}
				info[id].shipsSinked = 0;
				info[enemyId].shipsSinked = 0;
				info[id].gameFaze = 2;
				info[enemyId].gameFaze = 2;
			}

			pthread_mutex_unlock(&mutex);
		}
		else if (msg.substr(0, 2) == "if" && info[id].gameFaze == 2)
		{
			
			msg.erase(msg.begin(), msg.begin() + 2);
			int idk = atoi(msg.c_str());
			pthread_mutex_lock(&mutex);
			if (info[enemyId].myFields[idk] == 0)
			{
				SendMessage("miss", players[id]);
				SendMessage("go", players[enemyId]);
			}
			else if (info[enemyId].myFields[idk] > 0)
			{
				bool sinked = true;
				for (int i = 0; i < 100; i++)
				{
					if (info[enemyId].myFields[i] == info[enemyId].myFields[idk] && i != idk)
					{
						sinked = false;
						break;
					}
				}
				info[enemyId].myFields[idk] *= -1;
				if (sinked)
				{
					info[id].shipsSinked++;
					if (info[id].shipsSinked == 10)
					{
						SendMessage("win", players[id]);
						SendMessage("lose", players[enemyId]);
						EndGame = true;
					}
					else
						SendMessage("sink", players[id]);
						
				}
				else
				{
					SendMessage("hit", players[id]);
				}

			}
			pthread_mutex_unlock(&mutex);

		}
		else if (msg == "exit")
		{
			SendMessage("exit", players[enemyId]);
			EndGame = true;
		}
		
	}

	printf("Umiera watek klienta %d \n",(*th_data).socket);
	pthread_mutex_lock(&mutex);
	players[(*th_data).id]=0;
	clients--;
	info[id].Username = "";
	for (int i = 0; i < 100; i++)
		info[id].myFields[i] = 0;
	pthread_mutex_unlock(&mutex);
	free(th_data);
    pthread_exit(NULL);
}

//funkcja obs³uguj¹ca po³¹czenie z nowym klientem
void handleConnection(int connection_socket_descriptor) {
    //wynik funkcji tworz¹cej w¹tek
	//dane, które zostan¹ przekazane do w¹tku
	thread_data_t *(t_data) = new thread_data_t;
	(*t_data).socket = 0;
	(*t_data).id = 0;
    int create_result = 0;
	pthread_mutex_lock(&mutex);
	clients++;
	for(int i = 0;i<GAME_NUMBER*2;i++)
	{
		if(players[i]== 0)
		{
			players[i]=connection_socket_descriptor;
			t_data->id = i;
			break;
		}
	}
	pthread_mutex_unlock(&mutex);
    //uchwyt na w¹tek
    pthread_t thread1;


	t_data->socket = connection_socket_descriptor;
    create_result = pthread_create(&thread1, NULL, ThreadBehavior, (void *)t_data);
    if (create_result){
       printf("B³¹d przy próbie utworzenia w¹tku, kod b³êdu: %d\n", create_result);
       exit(-1);
    }
	printf("Stworzono uchwyt dla klienta %d\n",connection_socket_descriptor);

}

int main(int argc, char* argv[])
{
   int server_socket_descriptor;
   int connection_socket_descriptor;
   int bind_result;
   int listen_result;
   char reuse_addr_val = 1;
   struct sockaddr_in server_address;

   //inicjalizacja gniazda serwera

   memset(&server_address, 0, sizeof(struct sockaddr));
   server_address.sin_family = AF_INET;
   server_address.sin_addr.s_addr = htonl(INADDR_ANY);
   server_address.sin_port = htons(SERVER_PORT);

   server_socket_descriptor = socket(AF_INET, SOCK_STREAM, 0);
   if (server_socket_descriptor < 0)
   {
       fprintf(stderr, "%s: B³¹d przy próbie utworzenia gniazda..\n", argv[0]);
       exit(1);
   }
   setsockopt(server_socket_descriptor, SOL_SOCKET, SO_REUSEADDR, (char*)&reuse_addr_val, sizeof(reuse_addr_val));

   bind_result = bind(server_socket_descriptor, (struct sockaddr*)&server_address, sizeof(struct sockaddr));
   if (bind_result < 0)
   {
       fprintf(stderr, "%s: B³¹d przy próbie dowi¹zania adresu IP i numeru portu do gniazda.\n", argv[0]);
       exit(1);
   }

   listen_result = listen(server_socket_descriptor, GAME_NUMBER*2);
   if (listen_result < 0) {
       fprintf(stderr, "%s: B³¹d przy próbie ustawienia wielkoœci kolejki.\n", argv[0]);
       exit(1);
   }

   while(1)
   {
		while(clients>=GAME_NUMBER*2);
		printf("Czekam na polaczenie...\n");
       connection_socket_descriptor = accept(server_socket_descriptor, NULL, NULL);
       if (connection_socket_descriptor < 0)
       {
           fprintf(stderr, "%s: B³¹d przy próbie utworzenia gniazda dla po³¹czenia.\n", argv[0]);
           exit(1);
       }

       handleConnection(connection_socket_descriptor);
   }

   close(server_socket_descriptor);
   return(0);
}
