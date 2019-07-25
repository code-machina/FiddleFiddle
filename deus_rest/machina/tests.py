from django.test import TestCase
from machina.models import PKey, PVal, DictModel, Error

import django
# ref1. https://docs.djangoproject.com/en/2.0/ref/exceptions/
#       `ref1` shows a django exceptions list
from django.core.exceptions import ObjectDoesNotExist
from django.db.utils import IntegrityError
# from django.db.transaction import TransactionManagementError
from django.utils import timezone
import uuid
import socket

# Create your tests here.


""" Issues #1. Test Records are not exist.
ref2. https://docs.djangoproject.com/en/2.0/topics/testing/overview/#the-test-database
`ref2` 에서 test-database 에 대한 내용을 확인할 수 있다.
"""

from django.db import connection
# If using cursor without "with" -- it must be closed explicitly:


def makeup():
    with connection.cursor() as cursor:
        cursor.execute('CREATE EXTENSION hstore WITH SCHEMA public')
        cursor.execute('CREATE EXTENSION citext WITH SCHEMA public')


class ModelTestCase(TestCase):
    """Test Whether or not Model is properly configured as I expected.

    """
    def setUp(self):
        # makeup()  # before doing test, you have to set-up something.
        k = PKey.objects.create(key='param1')
        PVal.objects.create(key=k, value='value1')

    def test_pkey_uniqueness(self):
        # try to add same keys
        with self.assertRaises(django.db.utils.IntegrityError):
            PKey.objects.create(key='param1')

    def test_pval_update_and_check_uniqueness(self):
        # Assumption #1 : we already know key and value to insert,
        #                 but we don't know whether or not there is same key or value
        # Ref1. http://bookofstranger.com/optimizing-django-orm-queries-for-best-performance/
        #       ORM 쿼리 최적화를 위해 Ref1 을 읽어볼 필요가 있다.
        # given : param1 : value1
        # check there is PKey records
        # k = PKey.objects.get(key='Param1') # 비효율적임
        with self.assertRaises(ObjectDoesNotExist):
            pval = PVal.objects.select_related('key').get(key__key="Param1", value="value2")

        pval = PVal.objects.select_related('key').get(key__key="Param1", value="value1")
        self.assertEqual(pval.value, 'value1')
        # update value
        pval.value = "Value1"
        pval.save()

        # try to retrieve object
        pval2 = PVal.objects.select_related('key').get(key__key="param1", value="Value1")
        self.assertEqual(pval2.value, 'Value1')

    def test_pval_create(self):
        # you couldn't create PVal object if you don't give foreign key `key`
        with self.assertRaises(IntegrityError):
            PVal.objects.create(value='value3')

        # with self.assertRaises 문으로 감싸도 에러가 발생하는 것을 확인하여 주석처리하였다.
        # you can't create empty PKey object
        # with self.assertRaises(TransactionManagementError):
        #     # 빈값으로 데이터를 생성하려고 하였으나, 아래와 같은 에러를 만났다.
        #     # jango.db.transaction.TransactionManagementError: An error occurred in the current transaction.
        #     # You can't execute queries until the end of the 'atomic' block.
        #     PKey.objects.create(key='')

    def test_delete_pkey_and_cascading(self):
        # add value3, and value 4
        pk = PKey.objects.get(key="Param1")
        PVal.objects.create(key=pk, value='value3')
        PVal.objects.create(key=pk, value='value4')

        # and then delete Param1 from pkeys
        pk.delete()
        with self.assertRaises(ObjectDoesNotExist):
            pv = PVal.objects.get(value='value4')
            self.assertIsInstance(pv, PVal)

        # passes all of the tests from now.


class HStoreTestCase(TestCase):
    def test_hstore_create(self):
        dm = DictModel.objects.create(body_param={
            'param1':'value1',
            'param2':'value2',
            'param3':'value3'
        })

        self.assertIsInstance(dm.body_param, dict)  # passed
        self.assertDictEqual(dm.body_param, {
            'param1':'value1',
            'param2':'value2',
            'param3':'value3'
        })

    def test_hstore_query(self):
        # ref1. https://docs.djangoproject.com/en/2.0/ref/contrib/postgres/fields/#django.contrib.postgres.fields.HStoreField
        # to implement 
        dm1 = DictModel.objects.create(body_param={
            'param1':'value1',
            'param2':'value2',
            'param3':'value3'
        })

        dm2 = DictModel.objects.create(body_param={
            'param1':'value1',
            'param2':'value4',
            'param3':'value5'
        })

        r1 = DictModel.objects.filter(body_param__values__contains=['value1'])
        self.assertEqual(len(r1), 2)
        r2 = DictModel.objects.filter(body_param__values__contains=['value4'])
        self.assertEqual(len(r2), 1)


class LoggingTestCase(TestCase):
    def test_insert_logging_with_datetime(self):
        # ref1. https://stackoverflow.com/questions/33287493/django-rest-framework-datetime-field-format
        # To insert datetime properly, see the `ref1`, stackoverflow.com
        # See, machina.models to show that you don't need to update DateTimeField for memorizing UpdatedDate
        def get_unique_machine_code():
            # hostname|ipv4|mac|ram
            # uuid.NAMESPACE_DNS
            # uuid.uuid5(uuid.NAMESPACE_DNS, str(uuid.getnode()))
            return str(uuid.uuid5(uuid.UUID(bytes=b'api.yes.cert.com'),socket.gethostname()))

        Error.objects.create(
            app='fiddler',
            name='Non Handled Issues',
            trace="Traceback: System.NonHandledException, \nn Error Blah Blah ",
            date=timezone.now(),
            client="192.168.3.169",
            machine=get_unique_machine_code(),
            hostname=socket.gethostname(),
            language=Error.CSHARP
        )

        row = Error.objects.get(client='192.168.3.169')
        print(row.machine)

        self.assertEqual(
            row.machine,
            str(
                uuid.uuid5(
                    uuid.UUID(bytes=b'api.yes.cert.com'),
                    socket.gethostname()
                )
            ),
            'machine field test finished'
        )

