from rest_framework import serializers
from machina.models import Test, PVal, Error, PKey, UniqueUrl


class TestSerializer(serializers.ModelSerializer):
    class Meta:
        model = Test
        fields = (
                'id',
                'created',
                'modfied',
                'req_header',
                'method',
                'full_url',
                'url',
                'url_param',
                'body_param',
                'client_ip',
                'client_port',
                'client_process',
                'res_header',
                'comment',
                'is_https',
                'has_body',
                'server_ip',
                'server_port',
                'hostname',
                'url_key',
                'param_hash_key',
                'param_key',
                'res_code'
        )


class UniqueUrlSerializer(serializers.ModelSerializer):
    class Meta:
        model = UniqueUrl
        fields = (
            'c_url',
            'p_hash',
            'p_value'
        )
