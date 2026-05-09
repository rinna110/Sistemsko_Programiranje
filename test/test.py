import requests
import threading
import time
import random
SERVER_URL="http://localhost:5050/"

TEST_FILES=[
    "test.txt",
    "image.bin",
    "data.bin",
    "results.txt"
]

#slanje jednog zahteva

def send_request(client_id,file_name):
    try:
        start=time.time()
        response=requests.get(SERVER_URL + file_name)
        elapsed=time.time()-start
        print(
            f"[CLIENT {client_id}] "
            f"File={file_name} | "
            f"Status={response.status_code} | "
            f"Size={len(response.content)} bytes | "
            f"Time={elapsed:.3f}s"
        )
    except Exception as e:
        print(f"[CLIENT {client_id}] ERROR -> {e}")

#konkurentni pristup
#vise korisnika istovremeno pristupa serveru

def concurrent_access_test():
    print("\n1.Concurrent access test\n")

    threads=[]
    for i in range(20):
        file_name=random.choice(TEST_FILES);
        t=threading.Thread(
            target=send_request,
            args=(i,file_name)
        )
        threads.append(t)
    for t in threads:
        t.start()
    for t in threads:
        t.join()

#prvi zahtev -> CACHE MISS
#drugi zahtev -> CACHE HIT

def cache_behavior_test():
    print("\n2.Cache behavior test")

    target_file="test.txt"
    print("First request:")
    send_request("Cache1",target_file)

    time.sleep(1)

    print("\nSecond request")
    send_request("Cache2",target_file)

#Cache stampede zastita
#vise niti trazi isti fajl istovremeno

def cache_stampede_test():
    print("\n3.Cache stampede test\n")

    threads=[]
    for i in range(50):
        t=threading.Thread(
            target=send_request,
             args=(f"STAMP-{i}", "image.bin")
         )
        threads.append(t)

    for t in threads:
        t.start()
    for t in threads:
        t.join()

#razliciti resursi
#svaki korisnik trazi drugi fajl

def different_resources_test():
    print("\n4.Different resources test\n")
    
    threads=[]
    for i, file_name in enumerate(TEST_FILES):
        t=threading.Thread(
            target=send_request,
            args=(f"RES-{i}", file_name)
        )
        threads.append(t)
    for t in threads:
        t.start()
    for t in threads:
        t.join()

#GLAVNI PROGRAM
if __name__=="__main__" :
    print("\n Test:\n")

    concurrent_access_test()
    time.sleep(2)
    cache_behavior_test()
    time.sleep(2)
    cache_stampede_test()
    time.sleep(2)
    different_resources_test()





