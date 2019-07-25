from django.db import models, migrations

from django.contrib.postgres.fields import HStoreField, JSONField, CITextField

# Create your models here.
"""
Tip Note:
https://wayhome25.github.io/django/2017/09/23/django-blank-null/
자, 개발자들이 가장 실수하는 부분은 CharField, TextField와 같은 
문자열 기반 필드에 null=True를 정의하는 것이다. 
이 같은 실수를 피해야한다. 
그렇지 않으면 “데이터 없음”에 대해 두 가지 값, 
즉 None 과 빈 문자열 을 갖게된다. 
“데이터 없음”에 대해 두 가지 값을 갖는 것은 중복이다. 
그리고 Null이 아닌 빈 문자열을 사용하는 것이 장고 컨벤션이다.
"""

from django.contrib.postgres.operations import HStoreExtension, CITextExtension
from django.utils import timezone
# Migration 클래스는 migrations 폴더에 들어갈 내용이다. 3.14 확인하여 적용완료
class Migration(migrations.Migration):
    """ref1. https://docs.djangoproject.com/en/2.0/ref/contrib/postgres/operations/
    `ref1` PostgreSQL extension 을 생성할 수 있다.
    근데 선언하는 위치가 정확한지는 모르겠다.

    """
    operations = [
        HStoreExtension(),
        CITextExtension()
    ]

# url_key = "A" , param_hash_key {"A"-"B", "A"-"C"} 
#
# class TestKey(models.Model):
#     create = models.DateTimeField(auto_now_add=True)
#     url_key = models.TextField(unique=True)
#     param_hash_key = models.TextField(unique=True)
#     param


# TODO : url_key, param_hash_key make unique key.
class Test(models.Model):
    """
    See, `ref1`: 'https://stackoverfloaw.com/questions/417142/what-is-the-maximum-length-of-a-url-in-different-browsers'

    A New Approach for Verifying URL Uniqueness in Web Crawlers, SPIRE, Pisa, October 19th, 2011
    http://homepages.dcc.ufmg.br/~nivio/cursos/ri15/transp/spire11.pdf

    There are some URL Styles.
    First, URL has a parameters
        http://
    Second, URL has only path

    Third, Two URLs have same path, but they use different parameters
        Category 1. http://test.com/test?param1=a&param2=b
        Category 2. http://test.com/test
                    param1=a&param2=b
                    or,
                    {'param1':'a', 'param2':'b'}
    Let's simulate the insert process to guess what will happen.

    Assumption 1. Just distinguish request as a
        If someone try to insert same request again and again

    Case Insensitive Text Field CITextField

    """

    """Trouble Shooting, Message " You are trying to add non-nullable field 'something'"
    모델을 수정 시 django 에서 추가된 필드에 대해 값을 입력해준다. 따라서,
    입력할 값이 지정되지 않았으므로 발생하는 에러이다. (합리적임) 
    default 값을 추가해준다.
    """

    """Tip, Set Create & updated date/time in your models: 
    See, https://www.djangorocks.com/snippets/set-created-updated-datetime-in-your-models.html
    
    Samples:
    <pre><code>
        class Blog(models.Model):
            title = models.CharField(max_length=100)
            added = models.DateTimeField(auto_now_add=True)
            updated = models.DateTimeField(auto_now=True)
    </code></pre>
    """

    # ::: Record Updated Information :::
    created = models.DateTimeField(auto_now_add=True)  # created at 180316T10:40:10
    modfied = models.DateTimeField(auto_now=True)  # updated at 180316T10:50:22

    # ::: Request Data :::
    req_header = HStoreField()  # dictionary types
    method = models.TextField(default='')  # ex, PUT, POST, GET, HEAD, OPTIONS, ...

    # Notes // full_url's max_length option follows RFC 2616 - section 3.2.1 , RFC 7230, See `ref1`
    full_url = CITextField() # ex. http://test.com/path/for/url?param1&param2
    url = CITextField()  # ex. /path/for/url?param1&param2
    url_param = HStoreField(null=True)  # param1: value1, param2: value2
    body_param = HStoreField(null=True)  # body_param1: value1, ...

    # ::: Client Information :::
    # Notes // IPAddressField is deprecated since django 1.7 release
    client_ip = models.GenericIPAddressField(null=True)  # client ip,
    client_port = models.IntegerField(null=True, blank=True)
    client_process = CITextField(blank=True, null=True)  # like, chrome/12131 {process_name}/{pid}

    # ::: Server Information :::
    hostname = CITextField(blank=True) # www.google.com
    server_ip = models.GenericIPAddressField(null=True)
    server_port = models.IntegerField(null=True, blank=True)

    # ::: Response Data :::
    # Field for ResponseHeader
    res_code = models.IntegerField(blank=True, default=-1, null=True)
    res_header = HStoreField()

    # ::: User Interaction :::
    # user can update commentary to notify information
    comment = models.TextField(default="", blank=True)

    # ::: The section of boolean field :::
    is_https = models.BooleanField(default=False)
    has_body = models.BooleanField(default=False)

    # ::: Unique Url Checker
    url_key = models.TextField(null=True)
    param_hash_key = models.TextField(unique=True, null=True)
    param_key = models.TextField(unique=True, null=True)

    class Meta:
        ordering = ('created', )

"""
https://code.djangoproject.com/ticket/24082
textfield 의 unique 옵션에 대한 Thread 이다. 

주요 내용:
- CharField 와 TextField 의 Performance 차이는 없다. 
- DBA 는 text 필드를 varchar(n) 필드보다 추천한다.
- 쟁점은 unique=True 와 db_index 의 조합이다. 
  db_index 에 True 또는 False 를 줄지에 대해서 고민해야 한다.
"""


class UniqueUrl(models.Model):
    """

    """
    # http://www.google.com/where/to/go?param1=abdc&param2=abcde
    # http_wwwgooglecomwheretogo
    c_url = models.TextField(unique=False)
    # sha256
    p_hash = models.TextField(unique=True)
    # param1|param2
    p_value = models.TextField()


class PVal(models.Model):
    """ 이전에 사용한 데이터를 저장하기 위해서 Key-Value 기반의
    단순 데이터를 저장한다.
    OneToMany Relation : ParamKey (1) - ParamValue (n)

    파라미터에 대응하는 데이터를 저장하기 위해서 어떠한 규칙을
    따라야 하는가?
    Encoding 이슈를 피하기 위해 다음과 같이 저장한다.

    약속 : UTF8 Encoding + URLEncoding

    그러나 어느 단계에서 이러한 규칙을 강제할 수 있는가?
    RestAPI 를 사용하는 사용자?
    Data 를 저장하는 단계에서?

    """
    key = models.ForeignKey('pkey', related_name='pvals', on_delete=models.CASCADE)
    value = models.TextField(blank=False, unique=True) # 반드시 값이 있어야 한다.


class Error(models.Model):
    """Handling exception for tracing a reason.
    """
    # ref1. https://docs.djangoproject.com/en/2.0/ref/models/fields/#django.db.models.Field.choices
    CSHARP = 'CSHARP'
    PY2 = 'PY2'
    PY3 = 'PY3'
    CPP = 'CPP'
    C   = 'C'
    JS  = 'JS'
    PSH = 'PSH'

    ERROR_BY_LANGUAGES = (
        (CSHARP, 'CSharp'),
        (PY2, 'Python2'),
        (PY3, 'Python3'),
        (CPP, 'C++'),
        (C, 'C'),
        (JS,'Javascript'),
        (PSH,'Powershell')
    )

    app = models.TextField()  # fiddler, or anythings
    created = models.DateTimeField(auto_now_add=True)  # created date

    name = models.TextField(default='')
    trace = models.TextField(default='')
    date = models.DateTimeField()

    client = models.GenericIPAddressField()
    hostname = models.TextField(default='')
    machine = models.TextField()  # something code to trace
    language = models.TextField(choices=ERROR_BY_LANGUAGES, default=CSHARP)  #


class PKey(models.Model):
    """ 이전에 사용한 데이터를 저장하기 위해서 Key-Value 기반의
    단순 데이터를 저장한다.

    TroubleShooting #1. 2018.03.14

    테스트 수행 시 database 에 hstore 및 citext 자료형이 없다는
    오류가 발생한다. 아래의 SQL 문을 실행시켜야 한다.

    그리고 DB 테스트를 수행할 때, default 로 test 데이터베이스를
    삭제하므로 -k 옵션을 주어 keep 하도록 해야한다.

    CREATE EXTENSION hstore WITH SCHEMA public;
    CREATE EXTENSION citext WITH SCHEMA public;

    $ python manage.py test -k machina.tests

    """
    key = CITextField(blank=False, unique=True)


class DictModel(models.Model):
    """ Test for HStoreField

    """
    body_param = HStoreField(null=True)