import os
import json
from bs4 import BeautifulSoup
import requests
from urllib.request import urlretrieve

agent = 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.116 Safari/537.36 Edge/15.15007'
# cookies = ''
header = {"User-Agent": agent}

per_num = 3

file_list = []
'''
HandleSingleItem
'''
class HandleSingleItem(object):
    def __init__(self, img_path = 'D:/WeiboFolder/'):
        if not os.path.exists(img_path):
            os.mkdir(img_path)
        self.img_path = img_path

    def start(self,urls):
        try:
            for url in urls:
                print('正在解析微博:{}\n'.format(url))
                self.handle(url)
            print('全部解析完成，开始下载\n')
            self.download_files()
            print('全部下载完成\n')
        except Exception as ex:
            print(ex)

    def download_files(self):
        thread_num = int( len(file_list) / per_num) + 1
        pass

    def download_file(self, file):
        pass

    def handle(self, url):
        # url = 'http://weibo.com/3139784387/F1FIAdWL5'
        left = url.rfind('/') + 1
        right = url.find('?')
        if right == -1:
            wid = url[left:]
        else:
            wid = url[left:right]
        url = 'http://m.weibo.cn/status/' + wid
        try:
            r = requests.get(url)
            soup = BeautifulSoup(r.text, 'html.parser')
            r = soup.find_all('script')[1].text
            r = r[r.find('render_data') + 15:]
            r = r[:-12]
            r = json.loads(r)
            pics = r['status']['pics']
            for p in pics:
                purl = p['large']['url']
                name = '{0}/{1}.jpg'.format(self.img_path, p['pid'])
                urlretrieve(purl, name)
        except Exception as e:
            print(e)
        # print('完成')

if __name__ == '__main__':
    a = HandleSingleItem()
    urls = ['http://weibo.com/1706439477/F0ZuA3ow5',
    'http://weibo.com/1706439477/F1AJigQfI',
    'http://weibo.com/1706439477/F1hVqEMAa?from=page_1005051706439477_profile&wvr=6&mod=weibotime',
    'http://weibo.com/1706439477/F1eft0Qe5?from=page_1005051706439477_profile&wvr=6&mod=weibotime',
    'http://weibo.com/1706439477/F14qg1sjm?from=page_1005051706439477_profile&wvr=6&mod=weibotime',
    ]
    for url in urls:
        a.handle(url)
    print('全部完成')