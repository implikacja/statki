all: serwer

serwer: serwer.o
	g++ -o serwer serwer.o -pthread

serwer.o: serwer.cpp
	g++ -c serwer.cpp -o serwer.o -pthread
