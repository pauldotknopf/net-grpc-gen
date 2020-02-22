#include "qtesttypesworker.h"
#include "gen.pb.h"
#include "gen.grpc.pb.h"
#include "roc-lib/qrocobjectadapter.h"
#include "protobuf-qjson/protobufjsonconverter.h"
using namespace Tests;
class Tests::QTestTypesWorkerPrivate
{
public:
	QTestTypesWorkerPrivate() : objectId(0) {}
	google::protobuf::uint64 objectId;
	std::unique_ptr<Tests::TestTypesObjectService::Stub> service;
	grpc::ClientContext objectRequestContext;
	std::unique_ptr<grpc::ClientReaderWriter<google::protobuf::Any, google::protobuf::Any>> objectRequest;
	
	void createObject()
	{
		objectRequest = service->Create(&objectRequestContext);
		google::protobuf::Any createResponseAny;
		if(!objectRequest->Read(&createResponseAny))
		{
			qCritical("Failed to read request from object creation.");
			objectRequest.release();
			return;
		}
		Tests::TestTypesCreateResponse createResponse;
		if(!createResponseAny.UnpackTo(&createResponse))
		{
			qCritical("Failed to unpack request from object creation.");
			objectRequest.release();
			return;
		}
		objectId = createResponse.objectid();
	}
	void releaseObject()
	{
		if(objectRequest != nullptr)
		{
			auto result = objectRequest->Finish();
			if(!result.ok()) { qCritical("Couldn't dispose of object: %s", result.error_message().c_str()); }
		}
	}
	bool isValid()
	{
		return objectRequest != nullptr;
	}
};
QTestTypesWorker::QTestTypesWorker() : QObject(nullptr), d_priv(new QTestTypesWorkerPrivate())
{
	auto channel = QRocObjectAdapter::getSharedChannel();
	if(channel == nullptr) { qWarning("Set the channel to use via QRocObjectAdapter::setSharedChannel(...)"); return; }
	d_priv->service = Tests::TestTypesObjectService::NewStub(channel);
	d_priv->createObject();
}
QTestTypesWorker::~QTestTypesWorker()
{
	d_priv->releaseObject();
}
void QTestTypesWorker::testParamDouble(double val, int requestId)
{
	QMetaObject::invokeMethod(this, [this, val, requestId] {
		Tests::TestTypesTestParamDoubleMethodRequest request;
		Tests::TestTypesTestParamDoubleMethodResponse response;
		request.set_objectid(d_priv->objectId);
		request.set_value(val);
		grpc::ClientContext context;
		auto invokeResult = d_priv->service->InvokeTestParamDouble(&context, request, &response);
		if(!invokeResult.ok()) {
			emit testParamDoubleDone(requestId, QString::fromStdString(invokeResult.error_message()));
		} else {
			emit testParamDoubleDone(requestId, QString());
		}
	});
}
void QTestTypesWorker::testParamFloat(float val, int requestId)
{
	QMetaObject::invokeMethod(this, [this, val, requestId] {
		Tests::TestTypesTestParamFloatMethodRequest request;
		Tests::TestTypesTestParamFloatMethodResponse response;
		request.set_objectid(d_priv->objectId);
		request.set_value(val);
		grpc::ClientContext context;
		auto invokeResult = d_priv->service->InvokeTestParamFloat(&context, request, &response);
		if(!invokeResult.ok()) {
			emit testParamFloatDone(requestId, QString::fromStdString(invokeResult.error_message()));
		} else {
			emit testParamFloatDone(requestId, QString());
		}
	});
}
void QTestTypesWorker::testParamInt(int val, int requestId)
{
	QMetaObject::invokeMethod(this, [this, val, requestId] {
		Tests::TestTypesTestParamIntMethodRequest request;
		Tests::TestTypesTestParamIntMethodResponse response;
		request.set_objectid(d_priv->objectId);
		request.set_value(val);
		grpc::ClientContext context;
		auto invokeResult = d_priv->service->InvokeTestParamInt(&context, request, &response);
		if(!invokeResult.ok()) {
			emit testParamIntDone(requestId, QString::fromStdString(invokeResult.error_message()));
		} else {
			emit testParamIntDone(requestId, QString());
		}
	});
}
void QTestTypesWorker::testParamUInt(quint32 val, int requestId)
{
	QMetaObject::invokeMethod(this, [this, val, requestId] {
		Tests::TestTypesTestParamUIntMethodRequest request;
		Tests::TestTypesTestParamUIntMethodResponse response;
		request.set_objectid(d_priv->objectId);
		request.set_value(val);
		grpc::ClientContext context;
		auto invokeResult = d_priv->service->InvokeTestParamUInt(&context, request, &response);
		if(!invokeResult.ok()) {
			emit testParamUIntDone(requestId, QString::fromStdString(invokeResult.error_message()));
		} else {
			emit testParamUIntDone(requestId, QString());
		}
	});
}
void QTestTypesWorker::testParamLong(qint64 val, int requestId)
{
	QMetaObject::invokeMethod(this, [this, val, requestId] {
		Tests::TestTypesTestParamLongMethodRequest request;
		Tests::TestTypesTestParamLongMethodResponse response;
		request.set_objectid(d_priv->objectId);
		request.set_value(val);
		grpc::ClientContext context;
		auto invokeResult = d_priv->service->InvokeTestParamLong(&context, request, &response);
		if(!invokeResult.ok()) {
			emit testParamLongDone(requestId, QString::fromStdString(invokeResult.error_message()));
		} else {
			emit testParamLongDone(requestId, QString());
		}
	});
}
void QTestTypesWorker::testParamULong(ulong val, int requestId)
{
	QMetaObject::invokeMethod(this, [this, val, requestId] {
		Tests::TestTypesTestParamULongMethodRequest request;
		Tests::TestTypesTestParamULongMethodResponse response;
		request.set_objectid(d_priv->objectId);
		request.set_value(val);
		grpc::ClientContext context;
		auto invokeResult = d_priv->service->InvokeTestParamULong(&context, request, &response);
		if(!invokeResult.ok()) {
			emit testParamULongDone(requestId, QString::fromStdString(invokeResult.error_message()));
		} else {
			emit testParamULongDone(requestId, QString());
		}
	});
}
void QTestTypesWorker::testParamBool(bool val, int requestId)
{
	QMetaObject::invokeMethod(this, [this, val, requestId] {
		Tests::TestTypesTestParamBoolMethodRequest request;
		Tests::TestTypesTestParamBoolMethodResponse response;
		request.set_objectid(d_priv->objectId);
		request.set_value(val);
		grpc::ClientContext context;
		auto invokeResult = d_priv->service->InvokeTestParamBool(&context, request, &response);
		if(!invokeResult.ok()) {
			emit testParamBoolDone(requestId, QString::fromStdString(invokeResult.error_message()));
		} else {
			emit testParamBoolDone(requestId, QString());
		}
	});
}
void QTestTypesWorker::testParamString(QJsonValue val, int requestId)
{
	QMetaObject::invokeMethod(this, [this, val, requestId] {
		Tests::TestTypesTestParamStringMethodRequest request;
		Tests::TestTypesTestParamStringMethodResponse response;
		request.set_objectid(d_priv->objectId);
		auto messageVal = new google::protobuf::StringValue();
		ProtobufJsonConverter::jsonValueToMessage(val, messageVal);
		request.set_allocated_value(messageVal);
		grpc::ClientContext context;
		auto invokeResult = d_priv->service->InvokeTestParamString(&context, request, &response);
		if(!invokeResult.ok()) {
			emit testParamStringDone(requestId, QString::fromStdString(invokeResult.error_message()));
		} else {
			emit testParamStringDone(requestId, QString());
		}
	});
}
void QTestTypesWorker::testParamByte(quint32 val, int requestId)
{
	QMetaObject::invokeMethod(this, [this, val, requestId] {
		Tests::TestTypesTestParamByteMethodRequest request;
		Tests::TestTypesTestParamByteMethodResponse response;
		request.set_objectid(d_priv->objectId);
		request.set_value(val);
		grpc::ClientContext context;
		auto invokeResult = d_priv->service->InvokeTestParamByte(&context, request, &response);
		if(!invokeResult.ok()) {
			emit testParamByteDone(requestId, QString::fromStdString(invokeResult.error_message()));
		} else {
			emit testParamByteDone(requestId, QString());
		}
	});
}
void QTestTypesWorker::testParamBytes(QByteArray val, int requestId)
{
	QMetaObject::invokeMethod(this, [this, val, requestId] {
		Tests::TestTypesTestParamBytesMethodRequest request;
		Tests::TestTypesTestParamBytesMethodResponse response;
		request.set_objectid(d_priv->objectId);
		request.set_value(val);
		grpc::ClientContext context;
		auto invokeResult = d_priv->service->InvokeTestParamBytes(&context, request, &response);
		if(!invokeResult.ok()) {
			emit testParamBytesDone(requestId, QString::fromStdString(invokeResult.error_message()));
		} else {
			emit testParamBytesDone(requestId, QString());
		}
	});
}
