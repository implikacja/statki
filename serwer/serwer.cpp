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
	int myFields[100];
	string Username;
};

int players[GAME_NUMBER*2];//sockety pod³¹czonych graczy

information info[10];

bool SendMessage(string message, int playerSocket)
{

	for (int i = 0; i < message.size(); i += BUF_SIZE)
	{
		string tmp = message.substr(i, BUF_SIZE);
		char* cmsg = new char[tmp.length() + 1];
		strcpy(cmsg, tmp.c_str());
		write(playerSocket, cmsg, strlen(cmsg));
		delete[] cmsg;
	}
	return true;
}


void *ThreadBehavior(void *t_data)
{
    thread_data_t *th_data = (struct thread_data_t*)t_data;
	char message[BUF_SIZE];
	bzero(message, BUF_SIZE);
	int id = (*th_data).id;
	int gameFaze = 0;//0-powitanie 1-przesyl statków 2-gra 3-wyniki
	while(read((*th_data).socket,message, sizeof(message))>0)
	{
		printf("Odbieram: %d %s\n", id, message);
		string msg(message);
		if (msg.substr(0, 2) == "hi" && gameFaze == 0)
		{
			pthread_mutex_lock(&mutex);
				if (info[id].Username.size()==0)
					info[id].Username = msg.substr(2);

                if(id % 2 == 0)
                {
					if ((info[id + 1].Username).size() > 0)
					{
						string answer = "hi" + info[id + 1].Username;
						SendMessage(answer, players[id]);
						SendMessage("hi" + info[id].Username, players[id + 1]);
						gameFaze = 1;
					}

                }
                else
                {
					if ((info[id - 1].Username).size() > 0)
					{
						string answer = "hi" + info[id - 1].Username;
						SendMessage(answer, players[id]);
						SendMessage("hi" + info[id].Username, players[id - 1]);
						gameFaze = 1;
					}

                }

			pthread_mutex_unlock(&mutex);
		}

		bzero(message, BUF_SIZE);

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
