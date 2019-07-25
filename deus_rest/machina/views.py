from django.shortcuts import render
from rest_framework import viewsets
from rest_framework.request import Request
from machina.models import Test, UniqueUrl
from machina.serializers import TestSerializer, UniqueUrlSerializer
from pprint import pprint
# Create your views here.


def get_client_ip(request):
    x_forwarded_for = request.META.get('HTTP_X_FORWARDED_FOR')
    if x_forwarded_for:
        # print("returning FORWARDED_FOR")
        ip = x_forwarded_for.split(',')[-1].strip()
    elif request.META.get('HTTP_X_REAL_IP'):
        # print("returning REAL_IP")
        ip = request.META.get('HTTP_X_REAL_IP')
    else:
        # print("returning REMOTE_ADDR")
        ip = request.META.get('REMOTE_ADDR')
    return ip

def get_url_key(request: Request):
    pprint(request.data, indent=4)


class TestViewSet(viewsets.ModelViewSet):
    queryset = Test.objects.all()
    serializer_class = TestSerializer

    def perform_create(self, serializer):
        get_url_key(self.request)
        serializer.save(client_ip=get_client_ip(self.request))


class UniqueUrlViewSet(viewsets.ModelViewSet):
    queryset = UniqueUrl.objects.all()
    serializer_class = UniqueUrlSerializer
